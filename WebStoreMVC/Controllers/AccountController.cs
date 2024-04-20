using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
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

[Route("Api/[controller]")]
public class AccountController : Controller
{
    private readonly IAuthService _authService;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly IConfiguration _configuration;
    private readonly UserManager<AppUser> _userManager;
    private readonly WebStoreContext _context;

    public AccountController(IAuthService authService, SignInManager<AppUser> signInManager,
        IConfiguration configuration, UserManager<AppUser> userManager, WebStoreContext context)
    {
        _authService = authService;
        _signInManager = signInManager;
        _configuration = configuration;
        _userManager = userManager;
        _context = context;
    }

    [HttpPost("SeedingRoles")]
    [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin")]
    public async Task<IActionResult> SeedingRoles()
    {
        return Ok(await _authService.SeedRoles());
    }

    [HttpPost("Registration")]
    public async Task<IActionResult> Registration([FromBody] RegisterDto registerDto)
    {
        /*if (!ModelState.IsValid)
        {
            return View(registerDto);
        }*/

        var registerResult = await _authService.Register(registerDto);

        if (registerResult.IsSucceed)
        {
            return Ok(registerResult);
        }

        return BadRequest(registerResult);
    }

    [HttpPost("Login")]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        /*if (!ModelState.IsValid)
        {
            return View(loginDto);
        }*/

        var loginResult = await _authService.Login(loginDto);

        if (loginResult.IsSucceed)
        {
            return Ok(loginResult);
        }

        return BadRequest(loginResult);
    }
    

    [HttpPost("MakeAdmin")]
    [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin")]
    public async Task<IActionResult> FromUserToAdmin([FromBody] UpdateDto updateRoleDto)
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
    [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin")]
    public async Task<IActionResult> FromAdminToUser([FromBody] UpdateDto updateDto)
    {
        var changeRoleResult = await _authService.FromAdminToUser(updateDto);

        if (changeRoleResult.IsSucceed)
        {
            return Ok(changeRoleResult);
        }

        return BadRequest(changeRoleResult);
    }
}