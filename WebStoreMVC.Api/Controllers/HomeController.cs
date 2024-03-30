using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebStoreMVC.DAL.Context;
using WebStoreMVC.Domain.Entities;
using WebStoreMVC.Dtos;
using WebStoreMVC.Models;

namespace WebStoreMVC.Controllers;

public class HomeController : Controller
{
    private readonly WebStoreContext _context;

    public HomeController(WebStoreContext context)
    {
        _context = context;
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
        var products = await _context.Products.Take(15).ToListAsync();
        return View(products);
    }
}