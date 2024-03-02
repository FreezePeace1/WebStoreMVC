using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebStoreMVC.DAL.Context;
using WebStoreMVC.Domain.Entities;
using WebStoreMVC.Models;

namespace WebStoreMVC.Controllers;

public class HomeController : Controller
{
    private readonly WebStoreContext _context;

    public HomeController(WebStoreContext context)
    {
        _context = context;
    }
    
    public async Task<ActionResult<List<Product>>> Index()
    {
        return View(await _context.Products.Distinct().Take(10).ToListAsync());
    }
    
}