using Microsoft.AspNetCore.Mvc;
using Nest;
using WebStoreMVC.Domain.Entities;
using WebStoreMVC.Dtos;
using WebStoreMVC.Services.Interfaces;


namespace WebStoreMVC.Controllers;

public class HomeController : Controller
{
    private readonly IHomeService _homeService;
    private readonly ISearchingProductsService _searchingProductsService;

    public HomeController(IHomeService homeService,ISearchingProductsService searchingProductsService)
    {
        _homeService = homeService;
        _searchingProductsService = searchingProductsService;
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
    
    public async Task<ActionResult<ResponseDto<List<Product>>>> Store(string searchString = null) {
        ResponseDto<List<Product>> products;
        
        if (searchString != null)
        {
            products = await _searchingProductsService.SearchingProducts(searchString);
        }
        else
        {
            products = await _homeService.Store();
        }

        if (products.Data == null)
        {
            return RedirectToAction("Store");
        }
        
        return View(products.Data);
    }
}