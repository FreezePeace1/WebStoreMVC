using Microsoft.EntityFrameworkCore;
using WebStoreMVC.DAL.Context;
using WebStoreMVC.Domain.Entities;
using WebStoreMVC.Dtos;
using WebStoreMVC.Services;
using Xunit;

namespace WebStoreMVC.Application.Tests.Services;

public class ProductsServiceTests
{
    private async Task<WebStoreContext> GetDbContext()
    {
        var opts = new DbContextOptionsBuilder<WebStoreContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
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
                    CategoryId = 1
                },
                new Product
                {
                    ProductId = 2,
                    ProductName = "Notebook",
                    Price = 50000,
                    CategoryId = 2
                },
                new Product
                {
                    ProductId = 3,
                    ProductName = "Camera",
                    Price = 30000,
                    CategoryId = 3
                },
                new Product
                {
                    ProductId = 4,
                    ProductName = "Computer",
                    Price = 80000,
                    CategoryId = 4
                },
                new Product
                {
                    ProductId = 5,
                    ProductName = "Headphone",
                    Price = 6000,
                    CategoryId = 5
                }
            );

            await dbContext.SaveChangesAsync();
        }

        return dbContext;
    }

    [Fact]
    public async Task GetAllProducts_Returns_AllProductsInDb()
    {
        int expected_productsCount = 5;
        string expected_lastProductName = "Headphone";
        int expected_productPrice = 30000;
        var dbContext = await GetDbContext();

        var productsService = new ProductsService(dbContext, null);
        var result = await productsService.GetAllProducts();
        var lastProduct = result.Data.Last();

        Assert.NotNull(result);
        Assert.IsType<ResponseDto<List<Product>>>(result);
        Assert.Equal(expected_productsCount, result.Data.Count);
        Assert.NotNull(result.Data.Find(x => x.Price == expected_productPrice));
        Assert.Equal(expected_lastProductName, lastProduct.ProductName);
    }

    [Fact]
    public async Task GetProductById_Returns_ExactProduct()
    {
        int expected_findedProductId = 2;
        string expected_productName = "Notebook";
        int expected_categoryId = 2;

        var dbContext = await GetDbContext();
        var productsService = new ProductsService(dbContext, null);
        var result = await productsService.GetProductById(expected_findedProductId);

        Assert.NotNull(result);
        Assert.IsType<ResponseDto<Product>>(result);
        Assert.Equal(expected_findedProductId, result.Data.ProductId);
        Assert.Equal(expected_productName, result.Data.ProductName);
        Assert.Equal(expected_categoryId, result.Data.CategoryId);
    }

    [Fact]
    public async Task GetProductById_Returns_NullResult()
    {
        int non_existent_productId = 7;
        int expected_ErrorCode = 11;
        string expected_ErrorMessage = "Товар не был найден";

        var dbContext = await GetDbContext();
        var productsService = new ProductsService(dbContext, null);
        var result = await productsService.GetProductById(non_existent_productId);

        Assert.Null(result.Data);
        Assert.Null(result.SuccessMessage);
        Assert.NotNull(result.ErrorCode);
        Assert.NotNull(result.ErrorMessage);
        Assert.False(result.IsSucceed);
        Assert.Equal(expected_ErrorMessage, result.ErrorMessage);
        Assert.Equal(expected_ErrorCode, result.ErrorCode);
    }

    [Fact]
    public async Task PostProduct_Returns_CustomErrors()
    {
        int expected_errorCode = 12;
        string expected_errorMessage = "Данный товар уже существует";
        var existed_product = new Product
        {
            ProductId = 3,
            ProductName = "Camera",
            Price = 30000,
            CategoryId = 3
        };

        var dbContext = await GetDbContext();
        var productsService = new ProductsService(dbContext, null);
        var result = await productsService.PostProduct(existed_product);

        Assert.Null(result.Data);
        Assert.Null(result.SuccessMessage);
        Assert.False(result.IsSucceed);
        Assert.Equal(expected_errorCode, result.ErrorCode);
        Assert.Equal(expected_errorMessage, result.ErrorMessage);
    }

    [Fact]
    public async Task PostProduct_Returns_AddedProduct()
    {
        string expected_successMessage = "Товары получены";
        var expected_addedProduct = new Product
        {
            ProductId = 10,
            ProductName = "Item",
            Price = 10000,
            CategoryId = 5
        };
        
        var dbContext = await GetDbContext();
        var productsService = new ProductsService(dbContext, null);
        var result = await productsService.PostProduct(expected_addedProduct);
        
        Assert.NotNull(result.SuccessMessage);
        Assert.True(result.IsSucceed);
        Assert.Equal(expected_successMessage,result.SuccessMessage);
        Assert.IsType<ResponseDto<Product>>(result);
    }

    [Fact]
    public async Task Edit_Returns_NullReferenceException()
    {
        var non_existed_productId = 20;
        
        var dbContext = await GetDbContext();
        var productsService = new ProductsService(dbContext, null);

        Assert.Throws<NullReferenceException>(() => productsService.Edit(non_existed_productId));
    }

    [Fact]
    public async Task Edit_Returns_Product()
    {
        int expected_findedProductId = 3;
        string expected_productName = "Camera";
        int expected_productPrice = 30000;

        var dbContext = await GetDbContext();
        var productService = new ProductsService(dbContext, null);
        var result = productService.Edit(expected_findedProductId);
        
        Assert.NotNull(result);
        Assert.IsType<Product>(result);
        Assert.Equal(expected_findedProductId,result.ProductId);
        Assert.Equal(expected_productName,result.ProductName);
        Assert.Equal(expected_productPrice,result.Price);
    }

    [Fact]
    public async Task EditProduct_Returns_CustomErrorsProductsAreNotFound()
    {
        int expected_errorCode = 11;
        string expected_errorMessage = "Товар не был найден";
        
        var dbContext = await GetDbContext();
        var productsService = new ProductsService(dbContext,null);
        var result = await productsService.EditProduct(null);
        
        Assert.Null(result.SuccessMessage);
        Assert.False(result.IsSucceed);
        Assert.Equal(expected_errorMessage,result.ErrorMessage);
        Assert.Equal(expected_errorCode,result.ErrorCode);
    }

    [Fact]
    public async Task DeleteProduct_Returns_CustomSuccessMessageProductsAreReceived()
    {
        int expected_deletedProduct = 1;
        string expected_successMessage = "Товары получены";
        
        var dbContext = await GetDbContext();
        var productsService = new ProductsService(dbContext, null);
        var result = await productsService.DeleteProduct(expected_deletedProduct);
        
        Assert.Null(result.ErrorMessage);
        Assert.Null(result.ErrorCode);
        Assert.True(result.IsSucceed);
        Assert.Equal(expected_successMessage,result.SuccessMessage);
    }
    
    [Fact]
    public async Task DeleteProduct_Returns_CustomErrorMessagesProductsAreNotFound()
    {
        string expected_errorMessage = "Товар не был найден";
        int expected_errorCode = 11;
        
        var dbContext = await GetDbContext();
        var productsService = new ProductsService(dbContext, null);
        var result = await productsService.DeleteProduct(null);
        
        Assert.Null(result.SuccessMessage);
        Assert.False(result.IsSucceed);
        Assert.Equal(expected_errorCode,result.ErrorCode);
        Assert.Equal(expected_errorMessage,result.ErrorMessage);
    }

    [Fact]
    public async Task GetProductByPage_Returns_ProductsList()
    {
        int expected_pageTurnedToOne = -1;
        int expected_productsCount = 5;
        string expected_lastProductName = "Headphone";
        int expected_findedComputerPrice = 80000;

        var dbContext = await GetDbContext();
        var productsService = new ProductsService(dbContext, null);

        var result = await productsService.GetProductByPage(expected_pageTurnedToOne);
        
        Assert.NotNull(result.Data);
        Assert.IsType<ResponseDto<List<Product>>>(result);
        Assert.Equal(expected_productsCount,result.Data.Count);
        Assert.NotNull(result.Data.Last(x => x.ProductName == expected_lastProductName));
        Assert.NotNull(result.Data.Find(x => x.Price == expected_findedComputerPrice));

    }
}