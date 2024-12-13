using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using WebStoreMVC.Domain.Entities;
using Cookie = WebStoreMVC.Domain.Entities.Cookie;

namespace WebStoreMVC.Components;

public class UserInfoViewComponent : ViewComponent
{
    public IViewComponentResult Invoke()
    {
        return !string.IsNullOrEmpty(HttpContext.Request.Cookies[Cookie.refreshToken])
               && User.Identity.IsAuthenticated
            ? View("UserInfo")
            : View();
    }
}