using Microsoft.AspNetCore.Mvc;
using WebStoreMVC.JSON;
using WebStoreMVC.Models;
using WebStoreMVC.Models.ViewModels;

namespace WebStoreMVC.Domain.Components;

public class SmallCartViewComponent : ViewComponent
{
    public IViewComponentResult Invoke()
    {
        List<CartItem> cart = HttpContext.Session.GetJson<List<CartItem>>("Cart");
        SmallCartVIewModel smallCart;

        if (cart == null || cart.Count == 0)
        {
            smallCart = null;
        }
        else
        {
            smallCart = new()
            {
                NumberOfItems = cart.Sum(x => x.Quantity),
                TotalAmount = cart.Sum(x => x.Quantity * x.Price)
            };
        }

        return View(smallCart);
    }
}