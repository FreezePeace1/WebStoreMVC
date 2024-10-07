using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using Newtonsoft.Json;
using WebStoreMVC.Application.JSON;
using WebStoreMVC.Application.Services;
using WebStoreMVC.DAL.Context;
using WebStoreMVC.Domain.Entities;
using WebStoreMVC.Models;
using WebStoreMVC.Models.ViewModels;
using Xunit;

namespace WebStoreMVC.Application.Tests.Services;

public class CartServiceTests
{
    /*private async Task<WebStoreContext> GetDbContext()
    {
        var opts = new DbContextOptionsBuilder<WebStoreContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var dbContext = new WebStoreContext(opts);
        await dbContext.Database.EnsureCreatedAsync();

        if (!await dbContext.Products.AnyAsync())
        {
            await dbContext.Products.AddRangeAsync(
                new Product
                {
                    ProductId = 1,
                    ProductName = "Smartphone",
                    Price = 15000,
                    CategoryId = 1,
                    CategoryName = "Smartphones"
                },
                new Product
                {
                    ProductId = 2,
                    ProductName = "Notebook",
                    Price = 50000,
                    CategoryId = 2,
                    CategoryName = "Notebooks"
                },
                new Product
                {
                    ProductId = 3,
                    ProductName = "Camera",
                    Price = 30000,
                    CategoryId = 3,
                    CategoryName = "Cameras"
                },
                new Product
                {
                    ProductId = 4,
                    ProductName = "Computer",
                    Price = 80000,
                    CategoryId = 4,
                    CategoryName = "Computers"
                },
                new Product
                {
                    ProductId = 5,
                    ProductName = "Headphone",
                    Price = 6000,
                    CategoryId = 5,
                    CategoryName = "Headphones"
                });
                
            await dbContext.SaveChangesAsync();
        }

        return dbContext;
    }

    [Fact]
    public async Task Index_returns_CartViewModel()
    {
        var dbContext = await GetDbContext();
        var context = new HttpContextAccessor();
        context.HttpContext.Session.SetJson("cart",new List<CartItem>()
        {
            new CartItem()
            {
                ProductName = "phone"
            },
            new CartItem()
            {
                ProductName = "headphone"
            },
            new CartItem()
            {
                ProductName = "tv"
            }
        });
        
        var cartService = new CartService(dbContext,context, null);
        
        var result = cartService.Index();
        
        Assert.NotNull(result.Data);
        
    }*/
    
}