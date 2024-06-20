using Microsoft.AspNetCore.Mvc;
using WebStoreMVC.Application.JSON;
using WebStoreMVC.DAL.Context;
using WebStoreMVC.Models;
using WebStoreMVC.Models.ViewModels;
using WebStoreMVC.Services.Interfaces;

namespace WebStoreMVC.Controllers;

public class CartController : Controller
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    } 

    public IActionResult Index()
    {
        var cartServiceIndex = _cartService.Index();

        if (cartServiceIndex.IsSucceed)
        {
            return View(cartServiceIndex.Data);   
        }

        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Add(int id)
    {
        var cartServiceAdd = await _cartService.Add(id);
        
        if (cartServiceAdd.IsSucceed)
        {
            TempData["Success"] = "The product has been added";

            return Redirect(Request.Headers["Referer"].ToString());
        }
        
        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Decrease(int id)
    {
        var cartServiceDecrease = await _cartService.Decrease(id);

        if (cartServiceDecrease.IsSucceed)
        {
            TempData["Success"] = "The product has been decreased";   
        }
        
        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Remove(int id)
    {
        var cartServiceRemove = await _cartService.Remove(id);
        
        return RedirectToAction("Index");
    }
    
    public IActionResult Clear()
    {
        var cartServiceClear = _cartService.Clear();
        
        return RedirectToAction("Index");
    }
}