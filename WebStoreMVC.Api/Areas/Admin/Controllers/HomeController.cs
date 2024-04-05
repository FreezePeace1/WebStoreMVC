using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebStoreMVC.Domain.Entities;

namespace WebStoreMVC.Areas.Admin.Controllers;

[Route("[controller]")]
[Area("Admin"), Authorize(Policy = "AdminCookie",Roles = UserRoles.ADMINISTRATOR)]
public class HomeController : Controller
{
    [Route("Index")]
    public IActionResult Index()
    {
        return View();
    }
}