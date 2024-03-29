using Microsoft.AspNetCore.Mvc;

namespace WebStoreMVC.Components;

[ViewComponent(Name = "UserInfo")]
public class UserInfoViewComponent : ViewComponent
{
    public IViewComponentResult Invoke()
    {
        return HttpContext.Request.Cookies["accessToken"] != null || HttpContext.Request.Cookies["refreshToken"] != null ? View("UserInfo") : View();
    }
}