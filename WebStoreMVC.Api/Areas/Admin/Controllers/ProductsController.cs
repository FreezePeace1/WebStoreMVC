using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebStoreMVC.DAL.Context;
using WebStoreMVC.Domain.Entities;

namespace WebStoreMVC.Areas.Admin.Controllers;

/*[ApiController]*/
[Route("[controller]")]
[Area("Admin"), Authorize(Roles = UserRoles.ADMINISTRATOR)]
public class ProductsController : Controller
{
    private readonly WebStoreContext _context;

    public ProductsController(WebStoreContext context)
    {
        _context = context;
    }

    [HttpGet("GetImage")]
    public IActionResult GetImage(string imageName)
    {
        var path = Path.Combine(Directory.GetCurrentDirectory(), "Images", imageName);
        var image = System.IO.File.OpenRead(path);

        return File(image, "image/png");
    }

    [HttpGet("GetAllProducts")]
    [Route("GetAllProducts")]
    public async Task<ActionResult<List<Product>>> GetAllProducts()
    {
        var productList = await _context.Products.AsNoTracking().Take(1000).ToListAsync();

        return View(productList);
    }

    [HttpGet("GetProductById")]
    public async Task<ActionResult<Product>?> GetProductById(int id)
    {
        var product = await _context.Products.FirstOrDefaultAsync(x => x.Id == id);

        return Ok(product);
    }
    
    [HttpGet]
    [Route("PostProduct")]
    public IActionResult PostProduct()
    {
        return View(new Product());
    }
    
    [HttpPost("PostProduct")]
    [Route("PostProduct")]
    public async Task<ActionResult<Product>> PostProduct(Product product)
    {
        
        if (product is null)
        {
            throw new ArgumentNullException(nameof(product));
        }
        
        if (await _context.Products.ContainsAsync(product))
        {
            return RedirectToAction("PostProduct","Products");
        }

        var lastProduct = await _context.Products.OrderByDescending(x => x.Id)
            .FirstAsync();

        var newProduct = new Product()
        {
            Article = product.Article,
            Colour = product.Colour,
            Description = product.Description,
            Hashtags = product.Hashtags,
            Id = lastProduct.Id + 1,
            Images = product.Images,
            Manufacturer = product.Manufacturer,
            Price = product.Price,
            ProductName = product.ProductName,
            Quantity = product.Quantity
        };
        
        await _context.AddAsync(newProduct);
        await _context.SaveChangesAsync();

        return RedirectToAction("GetAllProducts","Products");
    }

    [HttpGet]
    [Route("EditProduct/{id}")]
    public IActionResult Edit(int id)
    {
        var product = _context.Products.FirstOrDefault(x => x.Id == id);
        
        return View(product);
    }

    [HttpPost("EditProduct")]
    [Route("EditProduct/{id}")]
    public async Task<IActionResult> EditProduct(Product product)
    {
        if (product == null)
        {
            return NotFound();
        }

        await _context.Products.Where(x => x.Id == product.Id).ExecuteUpdateAsync(s => s
            .SetProperty(p => p.Id, product.Id)
            .SetProperty(c => c.Description, product.Description)
            .SetProperty(p => p.Manufacturer, product.Manufacturer)
            .SetProperty(p => p.Colour, product.Colour)
            .SetProperty(p => p.ProductName, product.ProductName)
            .SetProperty(p => p.Article, product.Article)
            .SetProperty(p => p.Quantity, product.Quantity)
            .SetProperty(p => p.Hashtags, product.Hashtags)
            .SetProperty(p => p.Images, product.Images)
            .SetProperty(p => p.Price, product.Price)
        );

        await _context.SaveChangesAsync();

        return RedirectToAction("GetAllProducts", "Products");
    }

    [HttpDelete("Delete")]
    [Route("Delete/{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var product = await _context.Products.FindAsync(id);

        if (product == null)
        {
            return NotFound("MobilePhones not found");
        }

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();

        return RedirectToAction("GetAllProducts", "Products");
    }

    [HttpGet("GetByPage")]
    public async Task<List<Product>> GetProductByPage(int page, int pageSize)
    {
        return await _context.Products.AsNoTracking().Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
    }
}