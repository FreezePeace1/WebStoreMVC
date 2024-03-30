using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebStoreMVC.Domain.Entities;

namespace WebStoreMVC.Areas.Admin.Controllers;

[Route("[controller]")]
[Area("Admin"),Authorize(Roles = "Admin")]
public class HomeController : Controller
{
    public IActionResult Products()
    {
        return View();
    }
}