using Microsoft.AspNetCore.Mvc;
using Nest;
using WebStoreMVC.Domain.Entities;
using WebStoreMVC.Dtos;
using WebStoreMVC.Models;
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

    public async Task<ActionResult<ResponseDto<Product>>> Product(int id)
    {
        var product = await _homeService.GetProductById(id);
        
        return View(product);
    }
    
    public async Task<ActionResult<ResponseDto<ProductSearchingModel>>> Store(string searchString = "",int currentPage = 1) {
        ResponseDto<ProductSearchingModel> products = await _searchingProductsService.SearchingProducts(searchString,currentPage);
        
        return View(products);
    }
}