using Microsoft.AspNetCore.Mvc;
using WebStoreMVC.Application.JSON;
using WebStoreMVC.Models;
using WebStoreMVC.Models.ViewModels;

namespace WebStoreMVC.Components;

public class SmallCartViewComponent : ViewComponent
{
    public IViewComponentResult Invoke()
    {
        List<CartItem> cart = HttpContext.Session.GetJson<List<CartItem>>("Cart");
        
        return View(cart);
    }
}