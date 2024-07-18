using Microsoft.AspNetCore.Mvc;
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
        Assert.Equal(expected_listCartItem_count,model.CartItems.Count);
        Assert.Equal(expected_lastCartItem_Id,model.CartItems.Last().ProductId);
        Assert.Equal(expected_lastCartItem_Name,model.CartItems.Last().ProductName);
        Assert.NotNull(result);
    }

    [Fact]
    public void Clear_Returns_RedirectToAction()
    {
        _cartServiceMock.Setup(x => x.Clear())
            .Returns(new ResponseDto());

        var controller = new CartController(_cartServiceMock.Object);
        var result = controller.Clear();
        Assert.IsType<RedirectToActionResult>(result);
    }
}