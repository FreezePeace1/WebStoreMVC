using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebStoreMVC.Domain.Entities;
using WebStoreMVC.Dtos;
using WebStoreMVC.Services;
using WebStoreMVC.Services.Interfaces;

namespace WebStoreMVC.Controllers;

[ApiController]
[Route("Api/[controller]")]
public class AccountController : Controller
{
    private readonly IAuthService _authService;
    private readonly SignInManager<IdentityUser> _signInManager;

    public AccountController(IAuthService authService,SignInManager<IdentityUser> signInManager)
    {
        _authService = authService;
        _signInManager = signInManager;
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
        var loginResult = await _authService.Login(loginDto);

        if (loginResult.IsSucceed)
        {
            return Ok(await _authService.Login(loginDto));
        }

        return Unauthorized(loginResult);
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