using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using WebStoreMVC.Controllers;
using WebStoreMVC.Domain.Entities;
using WebStoreMVC.Dtos;
using WebStoreMVC.Models;
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
    public async Task Product_Returns_View()
    {
        int expected_productId = 10;
        decimal expected_productPrice = 15000;
        _homeServiceMock.Setup(x => x.GetProductById(It.IsAny<int>()))
            .ReturnsAsync(new ResponseDto<Product>()
            {
                Data = new Product()
                {
                    ProductId = 10,
                    Price = 15000
                }
            });
        
        var controller = new HomeController(_homeServiceMock.Object, _searchingProductsServiceMock.Object);

        var result = await controller.Product(It.IsAny<int>());

        var view_result = Assert.IsType<ViewResult>(result.Result);
        var model = Assert.IsAssignableFrom<ResponseDto<Product>>(view_result.Model);
        Assert.NotNull(model.Data);
        Assert.Equal(expected_productId,model.Data.ProductId);
        Assert.Equal(expected_productPrice,model.Data.Price);
    }

    [TestMethod]
    public async Task Store_Returns_View_With_Products()
    {
        _searchingProductsServiceMock.Setup(service => service.SearchingProducts(It.IsAny<string>(),It.IsAny<int>()))
            .ReturnsAsync(new ResponseDto<ProductSearchingModel>()
            {
                Data = new ProductSearchingModel()
            });

        var controller = new HomeController(_homeServiceMock.Object,_searchingProductsServiceMock.Object);
        
        var result = await controller.Store(It.IsAny<string>(),It.IsAny<int>());
        var view_result = Assert.IsType<ViewResult>(result.Result);
        var model = Assert.IsAssignableFrom<ResponseDto<ProductSearchingModel>>(view_result.Model);

        Assert.NotNull(model.Data);
        
        _homeServiceMock.VerifyNoOtherCalls();

    }
}