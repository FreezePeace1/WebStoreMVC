using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using WebStoreMVC.Controllers;
using WebStoreMVC.Domain.Entities;
using WebStoreMVC.Dtos;
using WebStoreMVC.Models;
using WebStoreMVC.Services.Interfaces;
using Xunit;
using Assert = Xunit.Assert;

namespace WebStoreMVC.Api.Home;

public class HomeControllerTests
{
    private readonly Mock<IHomeService> _homeServiceMock = new();
    private readonly Mock<ISearchingProductsService> _searchingProductsServiceMock = new();
    private readonly Mock<IReviewService> _reviewServiceMock = new();

    [Fact]
    public void Index_Returns_View()
    {
        var controller = new HomeController(_homeServiceMock.Object, _searchingProductsServiceMock.Object,
            _reviewServiceMock.Object);

        var result = controller.Index();

        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public void Checkout_Returns_View()
    {
        var controller = new HomeController(_homeServiceMock.Object, _searchingProductsServiceMock.Object,
            _reviewServiceMock.Object);

        var result = controller.Checkout();

        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public void Blank_Returns_View()
    {
        var controller = new HomeController(_homeServiceMock.Object, _searchingProductsServiceMock.Object,
            _reviewServiceMock.Object);

        var result = controller.Blank();

        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public async Task Product_Returns_View()
    {
        int expected_productId = 10;
        decimal expected_productPrice = 15000;
        _homeServiceMock.Setup(x => x.ShowProductInfo(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(new ResponseDto<AllInfoProductModel>()
            {
                Data = new AllInfoProductModel()
                {
                    ProductInfo = new Product()
                    {
                        ProductId = 10,
                        Price = 15000
                    }
                }
            });

        var controller = new HomeController(_homeServiceMock.Object, _searchingProductsServiceMock.Object,
            _reviewServiceMock.Object);

        var result = await controller.Product(expected_productId);

        var view_result = Assert.IsType<ViewResult>(result.Result);
        var model = Assert.IsAssignableFrom<ResponseDto<AllInfoProductModel>>(view_result.Model);
        Assert.NotNull(model.Data);
        Assert.Equal(expected_productId, model.Data.ProductInfo.ProductId);
        Assert.Equal(expected_productPrice, model.Data.ProductInfo.Price);
    }
    
    [Fact]
    public async Task Store_Returns_View_With_Products()
    {
        _searchingProductsServiceMock.Setup(service => service.SearchingProducts(It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(new ResponseDto<ProductSearchingModel>()
            {
                Data = new ProductSearchingModel()
            });

        var controller = new HomeController(_homeServiceMock.Object, _searchingProductsServiceMock.Object,
            _reviewServiceMock.Object);

        var result = await controller.Store(It.IsAny<string>(), It.IsAny<int>());
        var view_result = Assert.IsType<ViewResult>(result.Result);
        var model = Assert.IsAssignableFrom<ResponseDto<ProductSearchingModel>>(view_result.Model);

        Assert.NotNull(model.Data);

        _homeServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task PostReview_Returns_Views_With_PostReviewModel()
    {
        _reviewServiceMock.Setup(x => x.PostReview(It.IsAny<PostReviewDto>(), It.IsAny<int>()))
            .ReturnsAsync(new ResponseDto<PostReviewDto>()
            {
                Data = new PostReviewDto()
                {
                    
                }
            });

        var controller = new HomeController(_homeServiceMock.Object, _searchingProductsServiceMock.Object,
            _reviewServiceMock.Object);

        var result = await controller.PostReview(It.IsAny<PostReviewDto>(), It.IsAny<int>());

        var redirectToAction_result = Assert.IsType<RedirectToActionResult>(result.Result);
        Assert.NotNull(redirectToAction_result);

    }
}