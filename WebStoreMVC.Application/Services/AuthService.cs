using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using WebStoreMVC.Application.Resources;
using WebStoreMVC.DAL.Context;
using WebStoreMVC.Domain.Entities;
using WebStoreMVC.Domain.Enum;
using WebStoreMVC.Dtos;
using WebStoreMVC.Models;
using WebStoreMVC.Services.Interfaces;

namespace WebStoreMVC.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly IConfiguration _configuration;
    private readonly WebStoreContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthService(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager,
        SignInManager<AppUser> signInManager, IConfiguration configuration,WebStoreContext context,IHttpContextAccessor httpContextAccessor)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _signInManager = signInManager;
        _configuration = configuration;
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<ResponseDto> SeedRoles()
    {
        var isUserRoleExists = await _roleManager.RoleExistsAsync(UserRoles.USER);
        var isAdminRoleExists = await _roleManager.RoleExistsAsync(UserRoles.ADMINISTRATOR);

        if (isUserRoleExists && isAdminRoleExists)
        {
            return new ResponseDto()
            {
                ErrorMessage = ErrorMessage.RolesAlreadyExists,
                ErrorCode = (int)ErrorCode.RolesAlreadyExists
            };
        }

        await _roleManager.CreateAsync(new IdentityRole(UserRoles.USER));
        await _roleManager.CreateAsync(new IdentityRole(UserRoles.ADMINISTRATOR));

        return new ResponseDto()
        {
            SuccessMessage = SuccessMessage.CreatingRolesIsDone,
        };
    }

    public async Task<ResponseDto> Register(RegisterDto registerDto)
    {
        //Проверяем существует ли пользователь 
        if (await _userManager.FindByNameAsync(registerDto.Username) is not null)
        {
            return new ResponseDto()
            {
                ErrorMessage = $"{ErrorMessage.UserAlreadyExists} ({registerDto.Username})",
                ErrorCode = (int)ErrorCode.UserAlreadyExists
            };
        }

        //Если пользователь существует,то создаем его
        var user = new AppUser
        {
            UserName = registerDto.Username,
            Email = registerDto.Email
        };

        var registerResult = await _userManager.CreateAsync(user, registerDto.Password);

        //Если создание прошло неудачно
        if (!registerResult.Succeeded)
        {
            var errors = registerResult.Errors.Select(e => e.Description);

            return new ResponseDto()
            {
                ErrorMessage = $"{ErrorMessage.ModelCreatingIsFalied} \n {errors}",
                ErrorCode = (int)ErrorCode.CreatingModelIsFailed
            };
        }

        //Добавляем роль пользователю
        await _userManager.AddToRoleAsync(user, UserRoles.USER);

        //Мы не запоминаем пользователя при регистрации
        await _signInManager.SignInAsync(user, false);

        return new ResponseDto()
        {
            SuccessMessage = $"{SuccessMessage.CreatingUserIsDone} ({user})",
        };
    }

    public async Task<ResponseDto> Login(LoginDto loginDto)
    {
        var user = await _userManager.FindByNameAsync(loginDto.Username);

        //Проверяем пароль и логин пользователя
        if (user is null)
        {
            return new ResponseDto()
            {
                ErrorMessage = ErrorMessage.IncorrectCredentials,
                ErrorCode = (int)ErrorCode.IncorrectCredentials
            };
        }

        if (!(await _userManager.CheckPasswordAsync(user, loginDto.Password)))
        {
            return new ResponseDto()
            {
                ErrorMessage = ErrorMessage.IncorrectCredentials,
                ErrorCode = (int)ErrorCode.IncorrectCredentials
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

        //Генерируем refresh token
        var refreshToken = GenerateRefreshToken();

        SetRefreshToken(refreshToken);

        user.RefreshToken = refreshToken.Token;
        user.TokenCreated = refreshToken.Created;
        user.TokenExpires = refreshToken.Expired;

        //Проверяем на срок истечения,если истекает то обновляем и записываем новый в БД
        if (user.TokenExpires < DateTime.Now)
        {
            var refreshTokenFromCookies = _httpContextAccessor.HttpContext.Request.Cookies["refreshToken"];

            if (!user.RefreshToken.Equals(refreshTokenFromCookies) || user.TokenExpires < DateTime.Now)
            {
                return new ResponseDto()
                {
                    ErrorMessage = ErrorMessage.IncorrectToken,
                    ErrorCode = (int)ErrorCode.IncorrectToken
                };
            }

            var newRefreshToken = GenerateRefreshToken();
            SetRefreshToken(newRefreshToken);
            user.RefreshToken = newRefreshToken.Token;
        }

        await _context.SaveChangesAsync();
        
        return new ResponseDto()
        {
            SuccessMessage = $"{token}",
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
    
    private RefreshToken GenerateRefreshToken()
    {
        var refreshToken = new RefreshToken
        {
            Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
            Expired = DateTime.Now.AddDays(30),
            Created = DateTime.Now
        };

        return refreshToken;
    }

    private void SetRefreshToken(RefreshToken refreshToken)
    {
        var cookieOpt = new CookieOptions
        {
            HttpOnly = true,
            //Для передачи только по https
            /*Secure = true,*/
            Expires = refreshToken.Expired
        };

        _httpContextAccessor.HttpContext.Response.Cookies.Append("refreshToken", refreshToken.Token, cookieOpt);
    }
    
    public async Task<ResponseDto> FromUserToAdmin(UpdateDto updateDto)
    {
        //Проверяем есть ли пользователь
        var user = await _userManager.FindByNameAsync(updateDto.Username);

        if (user is null)
        {
            return new ResponseDto()
            {
                ErrorMessage = $"{ErrorMessage.UserDoesNotExist} ({updateDto.Username})",
                ErrorCode = (int)ErrorCode.UserDoesNotExist
            };
        }
        
        if (updateDto.Username == AdminUser.ADMINNAME)
        {
            return new ResponseDto
            {
                ErrorMessage = $"{ErrorMessage.UserAlreadyIsAdmin} ({updateDto.Username})",
                ErrorCode = (int)ErrorCode.UserAlreadyIsAdmin
            };
        }
        await _userManager.AddToRoleAsync(user, UserRoles.ADMINISTRATOR);

        return new ResponseDto()
        {
            SuccessMessage = $"{SuccessMessage.UpgradeUserToAdmin} ({user})",
        };
    }
    
    public async Task<ResponseDto> FromAdminToUser(UpdateDto updateDto)
    {
        var user = await _userManager.FindByNameAsync(updateDto.Username);
        
        if (user is null)
        {
            return new ResponseDto
            {
                ErrorMessage = $"{ErrorMessage.UserDoesNotExist} ({updateDto.Username})",
                ErrorCode = (int)ErrorCode.UserDoesNotExist
            };
        }

        if (updateDto.Username == AdminUser.ADMINNAME)
        {
            return new ResponseDto
            {
                ErrorMessage = ErrorMessage.AccessError,
                ErrorCode = (int)ErrorCode.AccessError
            };
        }
        
        await _userManager.RemoveFromRoleAsync(user, UserRoles.ADMINISTRATOR);

        return new ResponseDto
        {
            SuccessMessage = $"{SuccessMessage.DowngradeAdminToUser} ({updateDto.Username})"
        };
    }
    
    public async Task<ResponseDto> Logout()
    {
        await _signInManager.SignOutAsync();

        return new ResponseDto()
        {
            SuccessMessage = SuccessMessage.LogoutIsDone 
        };
    }
}