using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using WebStoreMVC.Domain.Entities;

namespace WebStoreMVC.Components;

[ViewComponent(Name = "UserInfo")]
public class UserInfoViewComponent : ViewComponent
{
    public IViewComponentResult Invoke()
    {
        return HttpContext.Request.Cookies[CookieName.accessTokenExpires].IsNullOrEmpty() == false &&
               HttpContext.Request.Cookies[CookieName.refreshToken].IsNullOrEmpty() == false &&
               User.Identity.IsAuthenticated
            ? View("UserInfo")
            : View();
    }
}