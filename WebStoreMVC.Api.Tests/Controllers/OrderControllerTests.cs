using Microsoft.AspNetCore.Mvc;
using Moq;
using Stripe;
using Stripe.Checkout;
using WebStoreMVC.Controllers;
using WebStoreMVC.Domain.Entities;
using WebStoreMVC.Dtos;
using WebStoreMVC.Models;
using WebStoreMVC.Services.Interfaces;
using Xunit;

namespace WebSotoreMVC.Api.Controllers;

public class OrderControllerTests
{
    private readonly Mock<IOrderService> _orderServiceMock = new();

    [Fact]
    public async Task SaveCustomerInfo_Returns_RedirectToAction()
    {
        var customerInfo = new CustomerInfoDto()
        {
            Address = "Pushkino dom kolotushkino"
        };
        
        _orderServiceMock.Setup(x => x.SaveCustomerInfo(It.IsAny<CustomerInfoDto>()))
            .ReturnsAsync(new ResponseDto<CustomerInfo>()
            {
                Data = new CustomerInfo()
                {
                    Address = "Pushkino dom kolotushkino"
                }
            });

        var controller = new OrderController(_orderServiceMock.Object);
        var result = await controller.SaveCustomerInfo(customerInfo);
        var redirectToAction_result = Assert.IsType<RedirectToActionResult>(result.Result);
    }

    [Fact]
    public void SuccessfulTransaction_Returns_View()
    {
        var controller = new OrderController(_orderServiceMock.Object);
        var result = controller.SuccessfulTransaction();
        Assert.IsType<ViewResult>(result);
    }
    
    [Fact]
    public void FailureTransaction_Returns_View()
    {
        var controller = new OrderController(_orderServiceMock.Object);
        var result = controller.FailureTransaction();
        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public void ShowCartInfo_Returns_ListCartItemWithView()
    {
        int expected_cartItems_count = 2;
        int expected_lastCartItem_id = 3;
        int expected_lastCartItem_quantity = 10;
        _orderServiceMock.Setup(x => x.ShowCartInfo())
            .Returns(new ResponseDto<List<CartItem>>()
            {
                Data = new List<CartItem>()
                {
                    new CartItem()
                    {
                        
                    },
                    new CartItem()
                    {
                        ProductId = 3,
                        Quantity = 10
                    }
                }
            });

        var controller = new OrderController(_orderServiceMock.Object);
        var result = controller.ShowCartInfo();
        var view_result = Assert.IsType<ViewResult>(result.Result);
        var model = Assert.IsAssignableFrom<ResponseDto<List<CartItem>>>(view_result.Model);
        Assert.NotNull(model);
        Assert.Equal(expected_cartItems_count,model.Data.Count);
        Assert.Equal(expected_lastCartItem_id,model.Data.Last().ProductId);
        Assert.Equal(expected_lastCartItem_quantity,model.Data.Last().Quantity);

    }
}