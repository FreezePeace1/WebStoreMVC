using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebStoreMVC.Dtos;
using WebStoreMVC.Services.Interfaces;

namespace WebStoreMVC.Controllers;

[Route("Api/[controller]")]
[ApiVersion("1.0")]
public class AccountController : Controller
{
    private readonly IAuthService _authService;
    
    /// <summary>
    /// DI сервиса AuthService
    /// </summary>
    /// <param name="authService"></param>
    public AccountController(IAuthService authService)
    {
        _authService = authService;
    }
    
    /// <summary>
    /// Создание ролей
    /// </summary>
    /// <param name=""></param>
    /// <remarks>
    ///     Request for creating roles
    ///     POST
    /// </remarks>
    /// <response code = "200">Создание ролей прошло успешно</response>
    /// <response code = "400">Создание ролей прошло неудачно</response>
    [HttpPost("SeedingRoles")]
    [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ResponseDto>> SeedingRoles()
    {
        return Ok(await _authService.SeedRoles());
    }
    
    /// <summary>
    /// Регистрация пользователя
    /// </summary>
    /// <param name="registerDto"></param>
    /// <remarks>
    ///     Request for user registration
    ///     POST
    ///     {
    ///         "username": "string",
    ///         "password": "string",
    ///         "confirmedPassword": "string",
    ///         "email": "string"
    ///     }
    /// </remarks>
    /// <response code = "200">Регистрация пользователя прошла успешно</response>
    /// <response code = "400">Регистрация пользователя прошла неудачно</response>
    [HttpPost("Registration")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Registration([FromBody] RegisterDto registerDto)
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

    /// <summary>
    /// Авторизация пользователя
    /// </summary>
    /// <param name="loginDto"></param>
    /// <remarks>
    ///     Request for user authorization
    ///     POST
    ///     {
    ///         "username": "string",
    ///         "password": "string"
    ///     }
    /// </remarks>
    /// <response code = "200">Авторизация пользователя прошла успешно</response>
    /// <response code = "400">Авторизация пользователя прошла неудачно</response>
    [HttpPost("Login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
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

    /// <summary>
    /// Перевод роли с обычного пользователя (User) к роли администратора (Admin)
    /// </summary>
    /// <param name="updateRoleDto"></param>
    /// <remarks>
    ///     Request for transferring User to Admin 
    ///     POST
    ///     {
    ///         "username": "string"
    ///     }
    /// </remarks>
    /// <response code = "200">Перевод пользователя к админу прошел успешно</response>
    /// <response code = "400">Перевод пользователя к админу прошел неудачно</response>
    [HttpPost("MakeAdmin")]
    [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ResponseDto>> FromUserToAdmin([FromBody] UpdateDto updateRoleDto)
    {
        var changeRoleResult = await _authService.FromUserToAdmin(updateRoleDto);

        if (changeRoleResult.IsSucceed)
        {
            return Ok(changeRoleResult);
        }

        return BadRequest(changeRoleResult);
    }

    /// <summary>
    /// Выход пользователя из аккаунта
    /// </summary>
    /// <remarks>
    ///     Request for log out User 
    ///     POST
    /// </remarks>
    /// <response code = "200">Выход пользователя прошел успешно</response>
    /// <response code = "400">Выход пользователя прошел неудачно</response>
    [HttpPost("Logout")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Logout()
    {
        var logoutResult = await _authService.Logout();

        if (logoutResult.IsSucceed)
        {
            return RedirectToAction("Index", "Home");   
        }

        return BadRequest();
    }

    /// <summary>
    /// Перевод роли с администратора (Admin) к роли пользователя (User)
    /// </summary>
    /// <remarks>
    ///     Request for transferring Admin to User  
    ///     POST
    ///     {
    ///         "username": "string"
    ///     }
    /// </remarks>
    /// <response code = "200">Перевод админа к пользователю прошел успешно</response>
    /// <response code = "400">Перевод админа к пользователю прошел неудачно</response>
    [HttpPost("DowngradeFromAdminToUser")]
    [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ResponseDto>> FromAdminToUser([FromBody] UpdateDto updateDto)
    {
        var changeRoleResult = await _authService.FromAdminToUser(updateDto);

        if (changeRoleResult.IsSucceed)
        {
            return Ok(changeRoleResult);
        }

        return BadRequest(changeRoleResult);
    }
}