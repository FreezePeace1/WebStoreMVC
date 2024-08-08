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
    private readonly IReviewService _reviewService;

    public HomeController(IHomeService homeService,ISearchingProductsService searchingProductsService,IReviewService reviewService)
    {
        _homeService = homeService;
        _searchingProductsService = searchingProductsService;
        _reviewService = reviewService;
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

    [HttpGet]
    public async Task<ActionResult<ResponseDto<AllInfoProductModel>>> Product(int id,int currentPage = 1)
    {
        var product = await _homeService.ShowProductInfo(id,currentPage);
        
        if (!ModelState.IsValid)
        {
            return View(product);
        }  
        
        if (product.IsSucceed)
        {
            return View(product);
        }
        
        if (!product.IsSucceed)
        {
            return RedirectToAction("Index");
        }
        
        return View("Index");
    }

    [HttpPost]
    public async Task<ActionResult<ResponseDto>> PostReview(PostReviewDto postReviewDto, int id)
    {
        if (!ModelState.IsValid)
        {
            return View(postReviewDto);
        }  
        
        var postReview = await _reviewService.PostReview(postReviewDto, id);
        
        if (postReview.IsSucceed)
        {
            return RedirectToAction("Product",id);
        }
        
        if (!postReview.IsSucceed)
        {
            var error = postReview.ErrorMessage;
            ModelState.AddModelError(string.Empty, error.ToString());
        }

        return View(postReviewDto);
    }
    
    public async Task<ActionResult<ResponseDto<ProductSearchingModel>>> Store(string searchString = "",int currentPage = 1) {
        ResponseDto<ProductSearchingModel> products = await _searchingProductsService.SearchingProducts(searchString,currentPage);
        
        return View(products);
    }
}