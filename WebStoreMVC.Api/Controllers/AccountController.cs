using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using WebStoreMVC.Domain.Entities;
using WebStoreMVC.Dtos;
using WebStoreMVC.Models;
using WebStoreMVC.Services.Interfaces;

namespace WebStoreMVC.Controllers;

[Authorize(Policy = "Default")]
[Route("[controller]")]
public class AccountController : Controller
{
    private readonly IAccountService _accountService;

    public AccountController(IAccountService accountService)
    {
        _accountService = accountService;
    }

    [HttpGet("Index")]
    [Route("Index")]
    public async Task<ActionResult<ResponseDto<List<ProductOrderModel>>>> Index()
    {
        if (HttpContext.Request.Cookies[CookieName.accessToken].IsNullOrEmpty() ||
            HttpContext.Request.Cookies[CookieName.refreshToken].IsNullOrEmpty() || !HttpContext.User.Identity.IsAuthenticated)
        {
            return RedirectToAction("Index", "Home");
        }

        var response = await _accountService.Index();

        return View(response);
    }

    [HttpPost("ChangeInfo")]
    [Route("ChangeInfo")]
    public async Task<ActionResult<ResponseDto>> ChangeInfo(CustomerInfoDto dto)
    {
        if (HttpContext.Request.Cookies[CookieName.accessToken].IsNullOrEmpty() ||
            HttpContext.Request.Cookies[CookieName.refreshToken].IsNullOrEmpty() || !HttpContext.User.Identity.IsAuthenticated)
        {
            return RedirectToAction("Index", "Home");
        }

        if (!ModelState.IsValid)
        {
            return View(dto);
        }

        var response = await _accountService.ChangeInfo(dto);

        if (response.IsSucceed)
        {
            return RedirectToAction("ShowInfo");
        }

        if (!response.IsSucceed)
        {
            var error = response.ErrorMessage;
            ModelState.AddModelError(string.Empty, error.ToString());
        }

        return View(response.Data);
    }

    [HttpGet("ShowInfo")]
    [Route("ShowInfo")]
    public async Task<ActionResult<ResponseDto<CustomerInfo>>> ShowInfo()
    {
        if (HttpContext.Request.Cookies[CookieName.accessToken].IsNullOrEmpty() ||
            HttpContext.Request.Cookies[CookieName.refreshToken].IsNullOrEmpty() || !HttpContext.User.Identity.IsAuthenticated)
        {
            return RedirectToAction("Index", "Home");
        }

        var response = await _accountService.ShowInfo();

        return View(response);
    }

    [HttpPost("ForgotPassword")]
    [Route("ForgotPassword")]
    public async Task<ActionResult<ResponseDto>> ForgotPassword()
    {
        var response = await _accountService.ForgotPassword();

        if (response.IsSucceed)
        {
            return RedirectToAction("ResetPassword");
        }

        return RedirectToAction("Index");
    }

    [HttpPost("ResetPassword")]
    [Route("ResetPassword")]
    public async Task<ActionResult<ResponseDto>> ResetPassword(ResetPasswordDto resetPasswordDto)
    {
        if (!ModelState.IsValid)
        {
            return View(resetPasswordDto);
        }
        var response = await _accountService.ResetPassword(resetPasswordDto);

        if (response.IsSucceed)
        {
            return RedirectToAction("PasswordChangedSuccessful");
        }
        
        if (!response.IsSucceed)
        {
            var error = response.ErrorMessage;
            ModelState.AddModelError(string.Empty, error.ToString());
        }

        return View(resetPasswordDto);
    }

    [HttpPost("PasswordChangedSuccessful")]
    [Route("PasswordChangedSuccessful")]
    public IActionResult PasswordChangedSuccessful()
    {
        return View();
    }
}