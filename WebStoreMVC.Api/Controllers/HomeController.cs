using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebStoreMVC.DAL.Context;
using WebStoreMVC.Domain.Entities;
using WebStoreMVC.Services.Interfaces;


namespace WebStoreMVC.Controllers;

public class HomeController : Controller
{
    private readonly IHomeService _homeService;

    public HomeController(IHomeService homeService)
    {
        _homeService = homeService;
    }
    public HomeController()
    {
        
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Checkout()
    {
        return View();
    }

    public IActionResult Blank()
    {
        return View();
    }

    public IActionResult Product()
    {
        return View();
    }

    public async Task<ActionResult<List<Product>>> Store()
    {
        var products = await _homeService.Store();
        return View(products);
    }
}