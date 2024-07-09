using Microsoft.AspNetCore.Mvc;
using WebStoreMVC.Services.Interfaces;

namespace WebStoreMVC.Controllers;

/// <summary>
/// Контроллер для сервиса корзины
/// </summary>
public class CartController : Controller
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    } 

    /// <summary>
    /// Возвращает View корзины пользователя
    /// </summary>
    /// <param name=""></param>
    /// <returns></returns>
    public IActionResult Index()
    {
        var cartServiceIndex = _cartService.Index();

        if (cartServiceIndex.IsSucceed)
        {
            return View(cartServiceIndex.Data);   
        }

        return RedirectToAction("Index");
    }

    /// <summary>
    /// Увеличиваем кол-во определенного товара в корзине на 1 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Уменьшаем кол-во определенного товара в корзине на 1
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<IActionResult> Decrease(int id)
    {
        var cartServiceDecrease = await _cartService.Decrease(id);

        if (cartServiceDecrease.IsSucceed)
        {
            TempData["Success"] = "The product has been decreased";   
        }
        
        return RedirectToAction("Index");
    }

    /// <summary>
    /// Удаляем полностью товар из корзины
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<IActionResult> Remove(int id)
    {
        var cartServiceRemove = await _cartService.Remove(id);
        
        return RedirectToAction("Index");
    }
    
    /// <summary>
    /// Чистим корзину полностью от товаров
    /// </summary>
    /// <param name=""></param>
    /// <returns></returns>
    public IActionResult Clear()
    {
        var cartServiceClear = _cartService.Clear();
        
        return RedirectToAction("Index");
    }
}