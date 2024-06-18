using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebStoreMVC.Domain.Entities;
using WebStoreMVC.Dtos;
using WebStoreMVC.Models;
using WebStoreMVC.Services.Interfaces;

namespace WebStoreMVC.Areas.Admin.Controllers;

/*[ApiController]*/
[Route("[controller]")]
[Area("Admin"), Authorize(Policy = "AdminCookie", Roles = UserRoles.ADMINISTRATOR)]
public class ProductsForAdminController : Controller
{
    private readonly IProductsService _productsService;
    private readonly ISearchingProductsService _searchingProductsService;

    /// <summary>
    /// DI сервиса CRUD товаров
    /// </summary>
    /// <param name="productsService"></param>
    public ProductsForAdminController(IProductsService productsService, ISearchingProductsService searchingProductsService)
    {
        _productsService = productsService;
        _searchingProductsService = searchingProductsService;
    }

    /// <summary>
    /// Получение всех товаров (
    /// </summary>
    /// <param name=""></param>
    /// <remarks>
    ///     Request for getting all products
    ///     GET
    /// </remarks>
    [HttpGet("GetAllProducts")]
    [Route("GetAllProducts")]
    public async Task<ActionResult<ResponseDto<List<Product>>>> GetAllProducts(int pg=1,string searchString = "")
    {
        ViewData["CurrentFilter"] = searchString;
        
        var productList = await _productsService.GetAllProducts();

        const int pageSize = 15;

        int rescCount = productList.Data.Count;
        var pager = new PagerModel(rescCount, pg, pageSize);
        
        productList = await _productsService.GetProductByPage(pg, pager.PageSize);
        
        this.ViewBag.Pager = pager;
        
        if (searchString != "")
        {
            productList = await _searchingProductsService.SearchingProducts(searchString);
        }

        if (productList.Data == null)
        {
            RedirectToAction("GetAllProducts");
        }
        
        return View(productList);
    }

    /// <summary>
    /// Получение товара по Id
    /// </summary>
    /// <param name=""></param>
    /// <remarks>
    ///     Request for getting product by id
    ///     GET
    ///     {
    ///         "id": "26"
    ///     }
    /// </remarks>
    /// <response code = "200">Получение товара прошло успешно</response>
    /// <response code = "500">Получение товара прошло неудачно</response>
    [HttpGet("GetProductById")]
    public async Task<ActionResult<Product>?> GetProductById(int id)
    {
        var product = await _productsService.GetProductById(id);

        return Ok(product);
    }


    [HttpGet]
    [Route("PostProduct")]
    public IActionResult PostProduct()
    {
        return View(new Product());
    }

    /// <summary>
    /// Создание товара
    /// </summary>
    /// <param name=""></param>
    /// <remarks>
    ///     Request for editing product
    ///     POST
    /// </remarks>
    [HttpPost("PostProduct")]
    [Route("PostProduct")]
    public async Task<ActionResult<Product>> PostProduct(Product product)
    {
        var postResult = await _productsService.PostProduct(product);

        if (postResult.IsSucceed)
        {
            return RedirectToAction("GetAllProducts", "ProductsForAdmin");
        }

        return RedirectToAction("PostProduct", "ProductsForAdmin");
    }

    [HttpGet]
    [Route("EditProduct/{id}")]
    public IActionResult EditProduct(int id)
    {
        var product = _productsService.Edit(id);

        return View(product);
    }

    /// <summary>
    /// Изменение товара
    /// </summary>
    /// <param name=""></param>
    /// <remarks>
    ///     Request for getting product by id
    ///     POST
    /// </remarks>
    /// <response code = "404">Товар не был найден</response> 
    [HttpPost("EditProduct")]
    [Route("EditProduct/{id}")]
    public async Task<IActionResult> EditProduct(Product product)
    {
        var editResult = await _productsService.EditProduct(product);

        if (!editResult.IsSucceed)
        {
            return NotFound("Product is not found");
        }

        return RedirectToAction("GetAllProducts", "ProductsForAdmin");
    }

    /// <summary>
    /// Удаление товара
    /// </summary>
    /// <param name=""></param>
    /// <remarks>
    ///     Request for deleting product
    ///     POST
    /// </remarks>
    /// <response code = "404">Товар не был найден</response> 
    [HttpPost("Delete")]
    [Route("Delete/{id}")]
    public async Task<IActionResult> DeleteProduct(int? id)
    {
        var deletingResult = await _productsService.DeleteProduct(id);

        if (deletingResult.IsSucceed)
        {
            return RedirectToAction("GetAllProducts", "ProductsForAdmin");
        }

        return NotFound("Product is not found");
    }

    /// <summary>
    /// Пагинация
    /// </summary>
    /// <param name=""></param>
    /// <remarks>
    ///     Request for getting product by page
    ///     GET
    /// </remarks>
    /// <response code = "200">Пагинация прошла успешно</response> 
    [HttpGet("GetByPage")]
    public async Task<IActionResult> GetProductByPage(int page, int pageSize)
    {
        var res = await _productsService.GetProductByPage(page, pageSize);

        return Ok(res);
    }
}