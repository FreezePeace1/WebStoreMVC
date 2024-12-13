using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using WebStoreMVC.Domain.Entities;
using WebStoreMVC.Dtos;
using WebStoreMVC.Services.Interfaces;

namespace WebStoreMVC.Controllers;

/// <summary>
/// Контроллер предназначенный для аутентификации и авторизации пользователей
/// </summary>
[Route("[controller]")]
/*[ApiController]
[ApiVersion("1.0")]*/
public class AuthController : Controller
{
    private readonly IAuthService _authService;

    /// <summary>
    /// DI сервиса AuthService
    /// </summary>
    /// <param name="authService"></param>
    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Создание ролей
    /// </summary>
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
    /// View для регистрации
    /// </summary>
    /// <returns>Возвращает view с RegisterDto</returns>
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
    /// <returns>При удачной отработке сервиса (записываем данные пользователя в БД и отправляем токен варификации на почту)
    /// переходим на начальную страницу, иначе будет возвращаться view до тех пор,
    /// пока пользователь не введет правильно данные для регистрации</returns>
    [HttpPost("Registration")]
    [Route("Registration")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Registration(RegisterDto registerDto)
    {
        if (HttpContext.Request.Cookies[Cookie.accessToken] != null &&
            HttpContext.Request.Cookies[Cookie.refreshToken] != null &&
            HttpContext.User.Identity.IsAuthenticated)
        {
            return RedirectToAction("Index", "Home");
        }

        if (!ModelState.IsValid)
        {
            return View(registerDto);
        }

        var registerResult = await _authService.Register(registerDto);

        if (!registerResult.IsSucceed)
        {
            var error = registerResult.ErrorMessage;
            ModelState.AddModelError(string.Empty, error.ToString());
        }

        if (registerResult.IsSucceed)
        {
            return RedirectToAction("Index", "Home");
        }

        return View(registerDto);
    }


    /// <summary>
    /// View для логина
    /// </summary>
    /// <returns>Возвращает view с LoginDto</returns>
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
    /// <returns>При удачной отработке сервиса и получения токена переходим на начальную страницу,
    /// если сервис отработал правильно, но пользователь не подтверждал варификацию, то переходим на страницу верификации,
    /// при этом отправляем повторное сообщение с токеном валидации на почту пользователя, иначе будет возвращаться view
    /// если пользователь неправильно вводит данные для аккаунта</returns>
    [HttpPost("Login")]
    [Route("Login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login([FromForm] LoginDto loginDto)
    {
        if (HttpContext.Request.Cookies[Cookie.accessToken] != null &&
            HttpContext.Request.Cookies[Cookie.refreshToken] != null &&
            HttpContext.User.Identity.IsAuthenticated)
        {
            return RedirectToAction("Index", "Home");
        }

        var loginResult = await _authService.Login(loginDto);

        if (!ModelState.IsValid)
        {
            return View(loginDto);
        }

        if (!loginResult.IsSucceed)
        {
            var error = loginResult.ErrorMessage;
            ModelState.AddModelError(string.Empty, error.ToString());
        }

        //Пользователь не активировал токен варификации
        if (loginResult.IsSucceed && loginResult.SuccessMessage == "")
        {
            return RedirectToAction("VerifyAccount");
        }

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
        await _authService.Logout();

        return RedirectToAction("Index","Home");
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

    /// <summary>
    /// Если пользователю отказали в доступе
    /// </summary>
    /// <returns></returns>
    public IActionResult AccessDenied()
    {
        return View();
    }

    /// <summary>
    /// Варификация через токен, который отправляется на почту пользователя
    /// </summary>
    /// <param name="verifyAccountDto"></param>
    /// <returns>При удачной отработке сервиса возвращает на начало страницы, иначе возвращает view ввода токена пока пользователь не введет правильный токен</returns>
    [HttpPost("VerifyAccount")]
    [Route("VerifyAccount")]
    public async Task<ActionResult<ResponseDto<VerifyAccountDto>>> VerifyAccount(VerifyAccountDto verifyAccountDto)
    {
        if (HttpContext.Request.Cookies[Cookie.accessToken] != null &&
            HttpContext.Request.Cookies[Cookie.refreshToken] != null &&
            HttpContext.User.Identity.IsAuthenticated)
        {
            return RedirectToAction("Index", "Home");
        }

        if (!ModelState.IsValid)
        {
            return View(verifyAccountDto);
        }

        var verifyService = await _authService.VerifyAccount(verifyAccountDto.VerificationToken);

        if (!verifyService.IsSucceed)
        {
            var error = verifyService.ErrorMessage;
            ModelState.AddModelError(string.Empty, error.ToString());
        }

        if (verifyService.IsSucceed)
        {
            return RedirectToAction("Index", "Home");
        }

        return View(verifyAccountDto);
    }

    /// <summary>
    /// Ввод почты пользователя и отправка токена на сброс пароля
    /// </summary>
    /// <param name="forgotPasswordDto"></param>
    /// <returns>При удачной отработке сервиса переходит на страницу сброса пароля, иначе пока пользователь не введет существующую почту</returns>
    [HttpPost("ForgotPassword")]
    [Route("ForgotPassword")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordDto forgotPasswordDto)
    {
        if (!ModelState.IsValid)
        {
            return View(forgotPasswordDto);
        }

        var forgotPasswordService = await _authService.ForgotPassword(forgotPasswordDto.Email);

        if (!forgotPasswordService.IsSucceed)
        {
            var error = forgotPasswordService.ErrorMessage;
            ModelState.AddModelError(string.Empty, error.ToString());
        }

        if (forgotPasswordService.IsSucceed)
        {
            return RedirectToAction("ResetPassword");
        }

        return View(forgotPasswordDto);
    }

    /// <summary>
    /// Сброс пароля засчет ввода токена для восстановления пароля, который отправляется на почту пользователя, самого нового пароля и его повторения
    /// </summary>
    /// <param name="resetPasswordDto"></param>
    /// <returns>При удачной отработке сервиса возвращает на начальную страницу, иначе пока не будут соблюдены условия: правильности токена и пароля</returns>
    [HttpPost("ResetPassword")]
    [Route("ResetPassword")]
    public async Task<IActionResult> ResetPassword(ResetPasswordDto resetPasswordDto)
    {
        if (!ModelState.IsValid)
        {
            return View(resetPasswordDto);
        }

        var resetPasswordService = await _authService.ResetPassword(resetPasswordDto);

        if (!resetPasswordService.IsSucceed)
        {
            var error = resetPasswordService.ErrorMessage;
            ModelState.AddModelError(string.Empty, error.ToString());
        }

        if (resetPasswordService.IsSucceed)
        {
            return RedirectToAction("Index", "Home");
        }

        return View(resetPasswordDto);
    }
}