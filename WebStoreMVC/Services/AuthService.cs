using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using WebStoreMVC.Domain.Entities;
using WebStoreMVC.Dtos;
using WebStoreMVC.Services.Interfaces;

namespace WebStoreMVC.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly IConfiguration _configuration;

    public AuthService(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager,
        SignInManager<IdentityUser> signInManager, IConfiguration configuration)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _signInManager = signInManager;
        _configuration = configuration;
    }

    public async Task<AuthResponseDto> SeedRoles()
    {
        var isUserRoleExists = await _roleManager.RoleExistsAsync(UserRoles.USER);
        var isAdminRoleExists = await _roleManager.RoleExistsAsync(UserRoles.ADMINISTRATOR);

        if (isUserRoleExists && isAdminRoleExists)
        {
            return new AuthResponseDto()
            {
                Message = "Роли уже созданы!",
                IsSucceed = true
            };
        }

        await _roleManager.CreateAsync(new IdentityRole(UserRoles.USER));
        await _roleManager.CreateAsync(new IdentityRole(UserRoles.ADMINISTRATOR));

        return new AuthResponseDto()
        {
            Message = "Создание ролей выполнено!",
            IsSucceed = true
        };
    }

    public async Task<AuthResponseDto> Register(RegisterDto registerDto)
    {
        //Проверяем существует ли пользователь 
        if (await _userManager.FindByNameAsync(registerDto.Username) is not null)
        {
            return new AuthResponseDto()
            {
                Message = $"{registerDto.Username} уже существует!",
                IsSucceed = false
            };
        }

        //Если пользователь существует,то создаем его
        var user = new IdentityUser
        {
            UserName = registerDto.Username,
            Email = registerDto.Email
        };

        var registerResult = await _userManager.CreateAsync(user, registerDto.Password);

        //Если создание прошло неудачно
        if (!registerResult.Succeeded)
        {
            var errors = registerResult.Errors.Select(e => e.Description);

            return new AuthResponseDto()
            {
                Message = $"{errors}",
                IsSucceed = false
            };
        }

        //Добавляем роль пользователю
        await _userManager.AddToRoleAsync(user, UserRoles.USER);

        //Мы не запоминаем пользователя при регистрации
        await _signInManager.SignInAsync(user, false);

        return new AuthResponseDto()
        {
            Message = $"Пользователь {user} создано успешно!",
            IsSucceed = true
        };
    }

    public async Task<AuthResponseDto> Login(LoginDto loginDto)
    {
        var user = await _userManager.FindByNameAsync(loginDto.Username);

        //Проверяем пароль и логин пользователя
        if (user is null)
        {
            return new AuthResponseDto()
            {
                Message = "Проверьте правильность логина или пароля",
                IsSucceed = false
            };
        }

        if (!(await _userManager.CheckPasswordAsync(user, loginDto.Password)))
        {
            return new AuthResponseDto()
            {
                Message = "Проверьте правильность логина или пароля",
                IsSucceed = false
            };
        }

        var userRoles = await _userManager.GetRolesAsync(user);

        //Создаем доп инфу для пользователя во время авторизации
        var claims = new List<Claim>()
        {
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim("JWTID", Guid.NewGuid().ToString())
        };

        //Передаем всем пользователям клеймы
        foreach (var userRole in userRoles)
        {
            claims.Add(new Claim(ClaimTypes.Role, userRole));
        }

        var token = GenerateJsonWebToken(claims);

        return new AuthResponseDto()
        {
            Message = $"{token}",
            IsSucceed = true
        };
    }

    private string GenerateJsonWebToken(List<Claim> claims)
    {
        var secret = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Key"]));

        var tokenObject = new JwtSecurityToken(
            issuer: _configuration["JwtSettings:Issuer"],
            audience: _configuration["JwtSettings:Audience"],
            expires: DateTime.Now.AddHours(12),
            claims: claims,
            signingCredentials: new SigningCredentials(secret, SecurityAlgorithms.HmacSha256));

        string token = new JwtSecurityTokenHandler().WriteToken(tokenObject);

        return token;
    }

    public async Task<AuthResponseDto> FromUserToAdmin(UpdateDto updateDto)
    {
        //Проверяем есть ли пользователь
        var user = await _userManager.FindByNameAsync(updateDto.Username);

        if (user is null)
        {
            return new AuthResponseDto()
            {
                Message = $"{updateDto.Username} не существует!",
                IsSucceed = false
            };
        }

        await _userManager.AddToRoleAsync(user, UserRoles.ADMINISTRATOR);

        return new AuthResponseDto()
        {
            Message = $"Now {user} стал Админом!",
            IsSucceed = true
        };
    }
}