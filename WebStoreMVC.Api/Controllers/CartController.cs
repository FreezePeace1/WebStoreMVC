using Microsoft.AspNetCore.Mvc;
using WebStoreMVC.DAL.Context;
using WebStoreMVC.JSON;
using WebStoreMVC.Models;
using WebStoreMVC.Models.ViewModels;

namespace WebStoreMVC.Controllers;

public class CartController : Controller
{
    private readonly WebStoreContext _dbcontext;

    public CartController(WebStoreContext dbcontext)
    {
        _dbcontext = dbcontext;
    } 

    public IActionResult Index()
    {
        List<CartItem> cart = HttpContext.Session.GetJson<List<CartItem>>("Cart")
                              ?? new List<CartItem>();

        CartViewModel cartVM = new()
        {
            CartItems = cart,
            GrandTotal = cart.Sum(x => x.Price * x.Quantity)
        };

        return View(cartVM);
    }

    public async Task<IActionResult> Add(int id)
    {
        var product = await _dbcontext.Products.FindAsync(id);

        if (product == null)
        {
            return RedirectToAction("Index");
        }

        List<CartItem> cart = HttpContext.Session.GetJson<List<CartItem>>("Cart")
                              ?? new List<CartItem>();

        CartItem cartItem = cart.Where(x => x.ProductId == id).FirstOrDefault();

        if (cartItem == null)
        {
            cart.Add(new CartItem(product));
        }
        else
        {
            cartItem.Quantity++;
        }
        
        HttpContext.Session.SetJson("Cart",cart);

        TempData["Success"] = "The product has been added";

        return Redirect(Request.Headers["Referer"].ToString());
    }

    public async Task<IActionResult> Decrease(int id)
    {
        List<CartItem> cart = HttpContext.Session.GetJson<List<CartItem>>("Cart")
                              ?? new List<CartItem>();
        
        CartItem cartItem = cart.Where(x => x.ProductId == id).FirstOrDefault();

        if (cartItem.Quantity > 1)
        {
            cartItem.Quantity--;
        }
        else
        {
            cart.RemoveAll(x => x.ProductId == id);
        }

        if (cart.Count == 0)
        {
            HttpContext.Session.Remove("Cart");
        }
        else
        {
            HttpContext.Session.SetJson("Cart",cart);
        }

        TempData["Success"] = "The product has been decreased";


        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Remove(int id)
    {
        List<CartItem> cart = HttpContext.Session.GetJson<List<CartItem>>("Cart");

        cart.RemoveAll(x => x.ProductId == id);

        if (cart.Count == 0)
        {
            HttpContext.Session.Remove("Cart");
        }
        else
        {
            HttpContext.Session.SetJson("Cart",cart);
        }

        return RedirectToAction("Index");
    }
    
    public IActionResult Clear()
    {
        HttpContext.Session.Remove("Cart");

        return RedirectToAction("Index");
    }
}