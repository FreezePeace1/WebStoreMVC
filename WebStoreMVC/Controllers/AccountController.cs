using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using WebStoreMVC.DAL.Context;
using WebStoreMVC.Domain.Entities;
using WebStoreMVC.Dtos;
using WebStoreMVC.Models;
using WebStoreMVC.Services;
using WebStoreMVC.Services.Interfaces;

namespace WebStoreMVC.Controllers;

[ApiController]
[Route("Api/[controller]")]
public class AccountController : Controller
{
    private readonly IAuthService _authService;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly IConfiguration _configuration;
    private readonly UserManager<AppUser> _userManager;
    private readonly WebStoreContext _context;

    public AccountController(IAuthService authService,SignInManager<AppUser> signInManager,IConfiguration configuration,UserManager<AppUser> userManager,WebStoreContext context)
    {
        _authService = authService;
        _signInManager = signInManager;
        _configuration = configuration;
        _userManager = userManager;
        _context = context;
    }
    
    [HttpPost("SeedingRoles")]
    [Authorize(Roles = UserRoles.ADMINISTRATOR)]
    public async Task<IActionResult> SeedingRoles()
    {
        return Ok(await _authService.SeedRoles());
    }

    [HttpPost("Registration")]
    public async Task<IActionResult> Registration(RegisterDto registerDto)
    {
        var registerResult = await _authService.Register(registerDto);

        if (registerResult.IsSucceed)
        {
            return Ok(registerResult);   
        }

        return BadRequest(registerResult);
    }

    [HttpPost("Login")]
    public async Task<IActionResult> Login(LoginDto loginDto)
    {
        var user = await _userManager.FindByNameAsync(loginDto.Username);

        //Проверяем пароль и логин пользователя
        if (user is null)
        {
            return Unauthorized("Проверьте правильность логина или пароля");
        }

        if (!(await _userManager.CheckPasswordAsync(user, loginDto.Password)))
        {
            return Unauthorized("Проверьте правильность логина или пароля");
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
        
        var refreshToken = GenerateRefreshToken();
        
        SetRefreshToken(refreshToken);

        user.RefreshToken = refreshToken.Token;
        user.TokenCreated = refreshToken.Created;
        user.TokenExpires = refreshToken.Expired;

        if (user.TokenExpires < DateTime.Now)
        {
            var refreshTokenFromCookies = Request.Cookies["refreshToken"];

            if (!user.RefreshToken.Equals(refreshTokenFromCookies) || user.TokenExpires < DateTime.Now)
            {
                return Unauthorized("Invalid refreshToken");
            }

            string newToken = GenerateJsonWebToken(claims);
            var newRefreshToken = GenerateRefreshToken();
            SetRefreshToken(newRefreshToken);

            user.RefreshToken = newRefreshToken.Token;

            return Ok(newToken);
        }
        await _context.SaveChangesAsync();

        return Ok(token);
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
            Expires = refreshToken.Expired
        };
        
        Response.Cookies.Append("refreshToken",refreshToken.Token,cookieOpt);
    }

    [HttpPost("MakeAdmin")]
    [Authorize(Roles = UserRoles.ADMINISTRATOR)]
    public async Task<IActionResult> FromUserToAdmin(UpdateDto updateRoleDto)
    {
        var changeRoleResult = await _authService.FromUserToAdmin(updateRoleDto);

        if (changeRoleResult.IsSucceed)
        {
            return Ok(changeRoleResult);
        }
        
        return BadRequest(changeRoleResult);
    }

    [HttpPost("Logout")]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();

        return RedirectToAction("Index", "Home");   
    }
    
    [HttpPost("DowngradeFromAdminToUser")]
    [Authorize(Roles = UserRoles.ADMINISTRATOR)]
    public async Task<IActionResult> FromAdminToUser(UpdateDto updateDto)
    {
        var changeRoleResult = await _authService.FromAdminToUser(updateDto);

        if (changeRoleResult.IsSucceed)
        {
            return Ok(changeRoleResult);
        }

        return BadRequest(changeRoleResult);
    }
}