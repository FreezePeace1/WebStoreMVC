using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using WebStoreMVC.Controllers;
using WebStoreMVC.Domain.Entities;
using WebStoreMVC.Dtos;
using WebStoreMVC.Services.Interfaces;
using Assert = Xunit.Assert;

namespace WebStoreMVC.Api.Home;

[TestClass]
public class HomeControllerTests
{
    private readonly Mock<IHomeService> _homeServiceMock = new();
    private readonly Mock<ISearchingProductsService> _searchingProductsServiceMock = new();
    
    [TestMethod]
    public void Index_Returns_View()
    {
        var controller = new HomeController(_homeServiceMock.Object, _searchingProductsServiceMock.Object);

        var result = controller.Index();

        Assert.IsType<ViewResult>(result);
    }

    [TestMethod]
    public void Checkout_Returns_View()
    {
        var controller = new HomeController(_homeServiceMock.Object, _searchingProductsServiceMock.Object);

        var result = controller.Checkout();

        Assert.IsType<ViewResult>(result);
    }

    [TestMethod]
    public void Blank_Returns_View()
    {
        var controller = new HomeController(_homeServiceMock.Object, _searchingProductsServiceMock.Object);

        var result = controller.Blank();

        Assert.IsType<ViewResult>(result);
    }

    [TestMethod]
    public void Product_Returns_View()
    {
        var controller = new HomeController(_homeServiceMock.Object, _searchingProductsServiceMock.Object);

        var result = controller.Product();

        Assert.IsType<ViewResult>(result);
    }

    [TestMethod]
    public async Task Store_Returns_View_With_Products()
    {
        const int expected_id_count = 15;
        const decimal expected_price_count = 15000M;
        const string expected_productName_Of_First_Product = "Smartphone";
        const int expected_taken_items_count = 5;
        
        var products = new List<Product>
        {
            new Product()
            {
                ProductId = 1,
                Price = 1000M,
                ProductName = "Smartphone"
            },
            new Product()
            {
                ProductId = 2,
                Price = 2000M
            },
            new Product()
            {
                ProductId = 3,
                Price = 3000M
            },
            new Product()
            {
                ProductId = 4,
                Price = 4000M
            },
            new Product()
            {
                ProductId = 5,
                Price = 5000M
            }
        };

        ResponseDto<List<Product>> resProducts = new()
        {
            Data = products
        };
        
        _homeServiceMock.Setup(service => service.Store())
            .ReturnsAsync(resProducts);

        var controller = new HomeController(_homeServiceMock.Object,_searchingProductsServiceMock.Object);
        
        var result = await controller.Store();
        var view_result = Assert.IsType<ViewResult>(result.Result);
        var model = Assert.IsAssignableFrom<List<Product>>(view_result.Model);

        Assert.NotNull(resProducts.Data);
        Assert.Equal(expected_id_count,model.Sum(x => x.ProductId));
        Assert.Equal(expected_price_count,model.Sum(x => x.Price));
        Assert.Equal(expected_productName_Of_First_Product,model.First().ProductName);
        Assert.Equal(expected_taken_items_count,model.Count);
        
        _homeServiceMock.Verify(service => service.Store());
        _homeServiceMock.VerifyNoOtherCalls();

    }
}