using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using Serilog;
using WebStoreMVC.Application.Services;
using WebStoreMVC.DAL.Context;
using WebStoreMVC.Domain.Entities;
using WebStoreMVC.Dtos;
using Xunit;

namespace WebStoreMVC.Application.Tests.Services;

public class CartServiceTests
{
    private readonly Mock<IHttpContextAccessor> _httpContextMock = new();
    private readonly Mock<ILogger> _loggerMock = new();

    /*[Fact]
    public async Task AddingItemsToCart()
    {
        int expected_product_id = 2;
        DbContextOptionsBuilder<WebStoreContext> optionsBuilder = new();
        optionsBuilder.UseInMemoryDatabase(MethodBase.GetCurrentMethod().Name);
        
        using (var dbContext = new WebStoreContext(optionsBuilder.Options))
        {
            await dbContext.Products.AddAsync(new Product()
            {
                ProductId = 2,
                ProductName = "Smartphone",
                Price = 10000,
                Manufacturer = "Xiaomi",
                CategoryId = 1
            });

            await dbContext.SaveChangesAsync();
        }
        
        CartService cartService;
        using (var dbContext = new WebStoreContext(optionsBuilder.Options))
        {
            cartService = new CartService(dbContext, _httpContextMock.Object, _loggerMock.Object);
        }

        var result = await cartService.Add(expected_product_id);
        
        var okResult = Assert.IsType<ResponseDto>(result);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Count.Should().Be(expected_product_id);
    }*/
}