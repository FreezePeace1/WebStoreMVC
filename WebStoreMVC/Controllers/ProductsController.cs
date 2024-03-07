using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebStoreMVC.DAL.Context;
using WebStoreMVC.Domain.Entities;

namespace WebStoreMVC.Controllers;

[ApiController]
[Route("Api/[controller]")]
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

    [HttpGet("GetProducts")]
    public async Task<List<Product>> GetProducts()
    {
        var productList = await _context.Products.AsNoTracking().ToListAsync();

        return productList;
    }

    [HttpGet("GetById")]
    public async Task<Product?> GetProductById(int id)
    {
        var product = await _context.Products.FirstOrDefaultAsync(x => x.Id == id);

        return product;
    }
    
    [HttpPost("PostMobilePhone")]
    public async Task<ActionResult<List<Product>>> PostMobilePhone(Product product)
    {
        _context.Add(product);
        await _context.SaveChangesAsync();
        return Ok(await _context.Products.ToArrayAsync());
    }

    [HttpPut("UpdateMobilePhone")]
    public async Task UpdateMobilePhone(Product product)
    {
        await _context.Products.Where(x => x.Id == product.Id).ExecuteUpdateAsync(s => s
            .SetProperty(p => p.Id,product.Id)
            .SetProperty(c => c.Description,product.Description)
            .SetProperty(p => p.Manufacturer,product.Manufacturer)
            .SetProperty(p => p.Colour,product.Colour)
            .SetProperty(p => p.ProductName,product.ProductName)
            .SetProperty(p => p.Article,product.Article)
            .SetProperty(p => p.Quantity,product.Quantity)
            .SetProperty(p => p.Hashtags,product.Hashtags)
            .SetProperty(p => p.Images,product.Images)
            .SetProperty(p => p.Price,product.Price)
        );

        await _context.SaveChangesAsync();
    }

    [HttpDelete("{id}")]
    public async Task DeleteMobilePhoneById(int id)
    {
        await _context.Products.Where(x => x.Id == id).ExecuteDeleteAsync();
        await _context.SaveChangesAsync();

    }

    [HttpGet("GetByPage")]
    public async Task<List<Product>> GetByPage(int page, int pageSize)
    {
        return await _context.Products.AsNoTracking().Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
    }
}