using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebStoreMVC.Dtos;
using WebStoreMVC.Services.Interfaces;

namespace WebStoreMVC.Controllers;

[Route("[controller]")]
/*[ApiController]
[ApiVersion("1.0")]*/
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
    
    public IActionResult Registration()
    {
        return View(new RegisterDto());
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
    [Route("Registration")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Registration(RegisterDto registerDto)
    {
        if (HttpContext.Request.Cookies["accessToken"] != null || HttpContext.Request.Cookies["refreshToken"] != null)
        {
            return RedirectToAction("Index", "Home");
        }
        
        if (!ModelState.IsValid)
        {
            return View(registerDto);
        }

        var registerResult = await _authService.Register(registerDto);

        var error = registerResult.ErrorMessage;
        ModelState.AddModelError(string.Empty, error.ToString());


        if (registerResult.IsSucceed)
        {
            return RedirectToAction("Index", "Home");
        }

        return View(registerDto);
    }


    public IActionResult Login()
    {
        return View(new LoginDto());
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
    [Route("Login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login([FromForm] LoginDto loginDto)
    {
        if (HttpContext.Request.Cookies["accessToken"] != null || HttpContext.Request.Cookies["refreshToken"] != null)
        {
            return RedirectToAction("Index", "Home");
        }
        
        if (!ModelState.IsValid)
        {
            return View(loginDto);
        }

        var loginResult = await _authService.Login(loginDto);

        if (loginResult.IsSucceed)
        {
            return RedirectToAction("Index", "Home");
        }

        return View(loginDto);
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
    [Route("MakeAdmin")]
    [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ResponseDto>> FromUserToAdmin([FromForm] UpdateDto updateRoleDto)
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
    [Route("Logout")]
    /*[ProducesResponseType(StatusCodes.Status200OK)]*/
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Logout()
    {
        if (HttpContext.Request.Cookies["accessToken"] == null || HttpContext.Request.Cookies["refreshToken"] == null)
        {
            return RedirectToAction("Index", "Home");
        }
        
        await _authService.Logout();

        return RedirectToAction("Index", "Home");
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
    [Route("DowngradeFromAdminToUser")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ResponseDto>> FromAdminToUser([FromForm] UpdateDto updateDto)
    {
        var changeRoleResult = await _authService.FromAdminToUser(updateDto);

        if (changeRoleResult.IsSucceed)
        {
            return Ok(changeRoleResult);
        }

        return BadRequest(changeRoleResult);
    }
}