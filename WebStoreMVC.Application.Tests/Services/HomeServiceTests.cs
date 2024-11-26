using Microsoft.EntityFrameworkCore;
using Moq;
using WebStoreMVC.DAL.Context;
using WebStoreMVC.Domain.Entities;
using WebStoreMVC.Dtos;
using WebStoreMVC.Models;
using WebStoreMVC.Services;
using WebStoreMVC.Services.Interfaces;
using Xunit;

namespace WebStoreMVC.Application.Tests.Services;

public class HomeServiceTests
{
    private async Task<WebStoreContext> GetDbContext()
    {
        var opts = new DbContextOptionsBuilder<WebStoreContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var dbContext = new WebStoreContext(opts);

        await dbContext.Database.EnsureCreatedAsync();

        if (!await dbContext.Products.AnyAsync() || !await dbContext.UserReviews.AnyAsync())
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

            await dbContext.UserReviews.AddRangeAsync(
                new UserReview
                {
                    AppUserId = "1",
                    Id = 1,
                    Rating = 4,
                    ProductId = 4,
                    ReviewDescription = "too good",
                    UserEmail = "1@gmail.com",
                    UserName = "First",
                    ReviewDateTime = new DateTime(2024,3,3)
                },
                new UserReview
                {
                    AppUserId = "2",
                    Id = 2,
                    Rating = 3,
                    ProductId = 4,
                    ReviewDescription = "not bad",
                    UserEmail = "2@gmail.com",
                    UserName = "Second",
                    ReviewDateTime = new DateTime(2024,5,13)
                },
                new UserReview
                {
                    AppUserId = "3",
                    Id = 3,
                    Rating = 5,
                    ProductId = 1,
                    ReviewDescription = "excellent",
                    UserEmail = "3@gmail.com",
                    UserName = "Third",
                    ReviewDateTime = new DateTime(2024,12,13)
                },
                new UserReview
                {
                    AppUserId = "4",
                    Id = 4,
                    Rating = 1,
                    ProductId = 1,
                    ReviewDescription = "won't buy it anymore",
                    UserEmail = "4@gmail.com",
                    UserName = "Fourth",
                    ReviewDateTime = new DateTime(2024,1,29)
                }
            );

            await dbContext.SaveChangesAsync();
        }

        return dbContext;
    }

    [Fact]
    public async Task Store_Returns_AllProducts()
    {
        int expected_productsAmount = 5;
        int expected_firstProductId = 1;
        string expected_lastProductName = "Headphone";

        var dbContext = await GetDbContext();
        var homeService = new HomeService(dbContext, null, null);
        var result = await homeService.Store();
        
        Assert.NotNull(result.Data);
        Assert.IsType<ResponseDto<List<Product>>>(result);
        Assert.Equal(expected_productsAmount,result.Data.Count);
        Assert.NotNull(result.Data.First(x => x.ProductId == expected_firstProductId));
        Assert.NotNull(result.Data.Last(x => x.ProductName == expected_lastProductName));
    }

    [Fact]
    public async Task ShowProductInfo_Returns_AllProductInfoWithFourthProductId()
    {
        int expected_productId = 4;
        string expected_productName = "Computer";
        int expected_productPrice = 80000;
        int expected_reviewsCount = 2;
        double expected_middleRating = 3.5;
        string expected_firstUserRewviewComment = "too good";

        var dbContext = await GetDbContext();
        var homeService = new HomeService(dbContext, null, null);
        var result = await homeService.ShowProductInfo(expected_productId);
        
        Assert.NotNull(result.Data);
        Assert.Equal(expected_productId,result.Data.ProductInfo.ProductId);
        Assert.Equal(expected_productName,result.Data.ProductInfo.ProductName);
        Assert.Equal(expected_productPrice,result.Data.ProductInfo.Price);
        Assert.Equal(expected_firstUserRewviewComment,result.Data.UserReviews.First(x => x.ProductId == expected_productId).ReviewDescription);
        Assert.Equal(expected_reviewsCount,result.Data.UserReviews.Count(x => x.ProductId == 4));
        Assert.Equal(expected_middleRating,result.Data.MiddleRateAmount);
        Assert.IsType<ResponseDto<AllInfoProductModel>>(result);
    }
}