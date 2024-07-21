using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using WebStoreMVC.Controllers;
using WebStoreMVC.Dtos;
using WebStoreMVC.Models;
using WebStoreMVC.Models.ViewModels;
using WebStoreMVC.Services.Interfaces;
using Xunit;
using Assert = Xunit.Assert;

namespace WebSotoreMVC.Api.Cart;

public class CartControllerTests
{
    private readonly Mock<ICartService> _cartServiceMock = new();

    [Fact]
    public void Index_Returns_View_With_Value()
    {
        int expected_listCartItem_count = 4;
        int expected_lastCartItem_Id = 13;
        string expected_lastCartItem_Name = "LCD";
        _cartServiceMock.Setup(x => x.Index())
            .Returns(new ResponseDto<CartViewModel>()
            {
                Data = new CartViewModel()
                {
                    CartItems = new List<CartItem>()
                    {
                        new CartItem()
                        {
                            ProductId = 120
                        },
                        new CartItem()
                        {
                        },
                        new CartItem()
                        {
                        },
                        new CartItem()
                        {
                            ProductId = 13,
                            ProductName = "LCD"
                        }
                    }
                }
            });
        var controller = new CartController(_cartServiceMock.Object);
        var result = controller.Index();
        var view_result = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<CartViewModel>(view_result.Model);
        Assert.NotNull(model.CartItems);
        Assert.Equal(expected_listCartItem_count, model.CartItems.Count);
        Assert.Equal(expected_lastCartItem_Id, model.CartItems.Last().ProductId);
        Assert.Equal(expected_lastCartItem_Name, model.CartItems.Last().ProductName);
        Assert.NotNull(result);
    }

    [Fact]
    public void Clear_Returns_RedirectResult()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["Referer"] = "test";
        _cartServiceMock.Setup(x => x.Clear())
            .Returns(new ResponseDto());

        var controller = new CartController(_cartServiceMock.Object)
        {
            ControllerContext =
            {
                HttpContext = httpContext
            }
        };

        var result = controller.Clear();
        Assert.IsType<RedirectResult>(result);
    }

    [Fact]
    public async Task Remove_Returns_RedirectResult()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["Referer"] = "test";
        _cartServiceMock.Setup(x => x.Remove(It.IsAny<int>()))
            .ReturnsAsync(new ResponseDto());

        var controller = new CartController(_cartServiceMock.Object)
        {
            ControllerContext =
            {
                HttpContext = httpContext
            }
        };
        var result = await controller.Remove(It.IsAny<int>());
        
        Assert.IsType<RedirectResult>(result);
    }
    
    [Fact]
    public async Task Decrease_Returns_RedirectToAction()
    {
        var httpContext = new DefaultHttpContext();
        var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
        tempData["Success"] = "test";
        _cartServiceMock.Setup(x => x.Decrease(It.IsAny<int>()))
            .ReturnsAsync(new ResponseDto());

        var controller = new CartController(_cartServiceMock.Object)
        {
            TempData = tempData
        };
        var result = await controller.Decrease(It.IsAny<int>());
        
        Assert.IsType<RedirectToActionResult>(result);
    }

    [Fact]
    public async Task Add_Returns_RedirectResult()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["Referer"] = "test";
        var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
        tempData["Success"] = "test";
        _cartServiceMock.Setup(x => x.Add(It.IsAny<int>()))
            .ReturnsAsync(new ResponseDto());

        var controller = new CartController(_cartServiceMock.Object)
        {
            TempData = tempData,
            ControllerContext =
            {
                HttpContext = httpContext
            }
        };
        
        var result = await controller.Add(It.IsAny<int>());
        Assert.IsType<RedirectResult>(result);

    }
}