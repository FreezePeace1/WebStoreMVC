using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebStoreMVC.DAL.Context;
using WebStoreMVC.Domain.Entities;

namespace WebStoreMVC.Controllers;

public class ProductsController : Controller
{
    private readonly WebStoreContext _context;
        public ProductsController(WebStoreContext context) {
            _context = context;
        }


        [HttpGet]
        public IActionResult GetImage(string imageName)
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Images", imageName);
            var image = System.IO.File.OpenRead(path);
            return File(image, "image/png");
        }


        [HttpPost]
        public async Task<ActionResult<List<Product>>> PostMobilePhones(Product product)
        {
            _context.Add(product);
            await _context.SaveChangesAsync();
            return Ok(await _context.Products.ToArrayAsync());
        }

        [HttpPut]
        public async Task<ActionResult<List<Product>>> UpdateMobilePhones(Product product)
        {
            var dbProduct = await _context.Products.FindAsync(product.Id);
            if(dbProduct == null) {
                return BadRequest("MobilePhones not found");
            }
            dbProduct.Price = product.Price;
            dbProduct.ProductName = product.ProductName;
            dbProduct.Quantity = product.Quantity;
            dbProduct.Description = product.Description;
            dbProduct.Manufacturer = product.Manufacturer;
            dbProduct.Colour = product.Colour;
            dbProduct.Hashtags = product.Hashtags;
            dbProduct.Images = product.Images;

            await _context.SaveChangesAsync();
            
            return Ok(await _context.Products.ToArrayAsync());
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<List<Product>>> DeleteMobilePhones(int id)
        {
            var dbProduct = await _context.Products.FindAsync(id);
            if (dbProduct == null)
            {
                return BadRequest("MobilePhones not found");
            }
            _context.Products.Remove(dbProduct);
            await _context.SaveChangesAsync();

            return Ok(await _context.Products.ToListAsync());
        }
        
        
    
}