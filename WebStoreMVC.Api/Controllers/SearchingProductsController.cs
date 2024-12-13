using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using WebStoreMVC.Domain.Entities;
using WebStoreMVC.Dtos;
using WebStoreMVC.Models;
using WebStoreMVC.Services.Interfaces;

namespace WebStoreMVC.Controllers;

[Route("[controller]")]
[ApiController]
[ApiVersion("1.0")]
public class SearchingProductsController : Controller
{
    private readonly ISearchingProductsService _productsService;

    public SearchingProductsController(ISearchingProductsService productsService)
    {
        _productsService = productsService;
    }


    [HttpGet("SearchingProducts")]
    [Route("SearchingProducts")]
    public async Task<ActionResult<ResponseDto<List<Product>>>> SearchingProducts(string searchString = "")
    {
        var searchingResult = await _productsService.SearchingProducts(searchString);

        return View(searchingResult);
    }

    [HttpGet("GetMobilePicture")]
    [Route("GetMobilePicture")]
    public ActionResult<FileContentResult> GetImage(string imageName)
    {
        imageName = imageName[0].ToString().ToUpper() + imageName.Substring(1);
        var path = Path.Combine(Directory.GetCurrentDirectory(), "Images", imageName);
        var image = System.IO.File.OpenRead(path + ".png");

        return File(image, "image/png");
    }
}