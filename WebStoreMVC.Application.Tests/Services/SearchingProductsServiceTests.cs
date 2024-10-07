using Microsoft.EntityFrameworkCore;
using WebStoreMVC.DAL.Context;
using WebStoreMVC.Domain.Entities;
using WebStoreMVC.Dtos;
using WebStoreMVC.Models;
using WebStoreMVC.Services;
using Xunit;

namespace WebStoreMVC.Application.Tests.Services;

public class SearchingProductsServiceTests
{
    private async Task<WebStoreContext> GetDbContext()
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
                    CategoryName = "Smartphones",
                    Manufacturer = "Xiaomi"
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
                },
                new Product
                {
                    ProductId = 6,
                    ProductName = "phone",
                    Price = 20000,
                    CategoryId = 1,
                    CategoryName = "Smartphones",
                    Manufacturer = "Xiaomi"
                }
            );

            await dbContext.SaveChangesAsync();
        }

        return dbContext;
    }

    [Fact]
    public async Task SearchingProducts_Returns_ProductsDependsOnString()
    {
        string expected_findedProductsByString = "phone xia";
        int expected_productsCount = 3;
        string expected_lastProductName = "phone";
        int expected_lastProductPrice = 20000;
        int expected_lastProductId = 6;
        int expected_firstProductId = 1;
        string expected_firstProductName = "Smartphone";
        string expected_productsManufacturer = "Xiaomi";
        string expected_findedProductName = "Headphone";

        var dbContext = await GetDbContext();
        var searchingProductsService = new SearchingProductsService(dbContext);
        var result = searchingProductsService.SearchingProducts(expected_findedProductsByString);
        
        Assert.NotNull(result.Result.Data.Products);
        Assert.Equal(expected_productsCount,result.Result.Data.Products.Count());
        Assert.IsType<ResponseDto<ProductSearchingModel>>(result.Result);
        Assert.True(result.Result.Data.Products.Count(x => x.ProductName == expected_findedProductName) == 1);
        Assert.NotNull(result.Result.Data.Products.Last(x => x.ProductName == expected_lastProductName));
        Assert.NotNull(result.Result.Data.Products.Last(x => x.Price == expected_lastProductPrice));
        Assert.NotNull(result.Result.Data.Products.Last(x => x.ProductId == expected_lastProductId));
        Assert.NotNull(result.Result.Data.Products.First(x => x.ProductId == expected_firstProductId));
        Assert.NotNull(result.Result.Data.Products.First(x => x.ProductName == expected_firstProductName));
        Assert.True(result.Result.Data.Products.Count(x => x.Manufacturer == expected_productsManufacturer) == 2);
    }
    
    [Fact]
    public async Task SearchingProducts_Returns_AllProducts()
    {
        int expected_productsCount = 6;
        string emptyString = "";
        
        var dbContext = await GetDbContext();
        var searchingProductsService = new SearchingProductsService(dbContext);
        var result = searchingProductsService.SearchingProducts(emptyString);
        
        Assert.NotNull(result.Result.Data.Products);
        Assert.Equal(expected_productsCount,result.Result.Data.Products.Count());
        Assert.IsType<ResponseDto<ProductSearchingModel>>(result.Result);
    }
}