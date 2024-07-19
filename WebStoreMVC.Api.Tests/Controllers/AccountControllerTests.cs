using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Moq;
using WebStoreMVC.Controllers;
using WebStoreMVC.Domain.Entities;
using WebStoreMVC.Dtos;
using WebStoreMVC.Models;
using WebStoreMVC.Services.Interfaces;
using Xunit;

namespace WebSotoreMVC.Api.Controllers;

public class AccountControllerTests
{
    private readonly Mock<IAccountService> _accountServiceMock = new();
    private readonly MockHttpContextAccessor _mockHttpContextAccessor = new();

    [Fact]
    public async Task Index_Returns_RedirectToAction()
    {
        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _mockHttpContextAccessor.HttpContextAccessorMockAutoSetupForRedirect(httpContextAccessorMock);

        var controller = new AccountController(_accountServiceMock.Object)
        {
            ControllerContext =
            {
                HttpContext = httpContextAccessorMock.Object.HttpContext
            }
        };

        var result = await controller.Index();
        Assert.IsType<RedirectToActionResult>(result.Result);
        Assert.IsAssignableFrom<ActionResult<ResponseDto<List<ProductOrderModel>>>>(result);
    }

    [Fact]
    public async Task Index_Returns_ListProductOrderWithView()
    {
        int expected_productsOrderModel_count = 2;
        string expected_lastProductOrderModel_address = "Pushkino dom kolotushkino";
        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _mockHttpContextAccessor.HttpContextAccessorMockAutoSetupToByPassRedirect(httpContextAccessorMock);
        _accountServiceMock.Setup(x => x.Index())
            .ReturnsAsync(new ResponseDto<List<ProductOrderModel>>()
            {
                Data = new List<ProductOrderModel>()
                {
                    new ProductOrderModel()
                    {
                    },
                    new ProductOrderModel()
                    {
                        Address = "Pushkino dom kolotushkino"
                    }
                }
            });

        var controller = new AccountController(_accountServiceMock.Object)
        {
            ControllerContext =
            {
                HttpContext = httpContextAccessorMock.Object.HttpContext
            }
        };

        var result = await controller.Index();
        var view_result = Assert.IsType<ViewResult>(result.Result);
        var model = Assert.IsAssignableFrom<ResponseDto<List<ProductOrderModel>>>(view_result.Model);
        Assert.NotNull(model.Data);
        Assert.Equal(expected_productsOrderModel_count, model.Data.Count);
        Assert.Equal(expected_lastProductOrderModel_address, model.Data.Last().Address);
    }

    [Fact]
    public async Task ChangeInfo_Returns_RedirectToActionWithSucceedService()
    {
        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _mockHttpContextAccessor.HttpContextAccessorMockAutoSetupToByPassRedirect(httpContextAccessorMock);

        var customerInfo = new CustomerInfoDto()
        {
        };

        _accountServiceMock.Setup(x => x.ChangeInfo(It.IsAny<CustomerInfoDto>()))
            .ReturnsAsync(new ResponseDto<CustomerInfoDto>()
            {
                Data = customerInfo
            });

        var controller = new AccountController(_accountServiceMock.Object)
        {
            ControllerContext =
            {
                HttpContext = httpContextAccessorMock.Object.HttpContext
            }
        };

        var result = await controller.ChangeInfo(customerInfo);
        Assert.IsType<RedirectToActionResult>(result.Result);
    }

    [Fact]
    public async Task ChangeInfo_Returns_RedirectToAction()
    {
        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _mockHttpContextAccessor.HttpContextAccessorMockAutoSetupForRedirect(httpContextAccessorMock);

        var customerInfo = new CustomerInfoDto()
        {
        };

        _accountServiceMock.Setup(x => x.ChangeInfo(It.IsAny<CustomerInfoDto>()))
            .ReturnsAsync(new ResponseDto<CustomerInfoDto>()
            {
                Data = customerInfo
            });

        var controller = new AccountController(_accountServiceMock.Object)
        {
            ControllerContext =
            {
                HttpContext = httpContextAccessorMock.Object.HttpContext
            }
        };

        var result = await controller.ChangeInfo(customerInfo);
        Assert.IsType<RedirectToActionResult>(result.Result);
    }

    [Fact]
    public async Task ShowInfo_Returns_RedirectToAction()
    {
        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _mockHttpContextAccessor.HttpContextAccessorMockAutoSetupForRedirect(httpContextAccessorMock);
        
        _accountServiceMock.Setup(x => x.ShowInfo());
        var controller = new AccountController(_accountServiceMock.Object)
        {
            ControllerContext =
            {
                HttpContext = httpContextAccessorMock.Object.HttpContext
            }
        };
        var result = await controller.ShowInfo();
        Assert.IsType<RedirectToActionResult>(result.Result);
    }
    
    [Fact]
    public async Task ShowInfo_Returns_CustomerInfoWithView()
    {
        string expected_firstName = "Alex";
        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _mockHttpContextAccessor.HttpContextAccessorMockAutoSetupToByPassRedirect(httpContextAccessorMock);
        
        _accountServiceMock.Setup(x => x.ShowInfo())
            .ReturnsAsync(new ResponseDto<CustomerInfo>()
            {
                Data = new CustomerInfo()
                {
                    FirstName = "Alex"
                }
            });
        var controller = new AccountController(_accountServiceMock.Object)
        {
            ControllerContext =
            {
                HttpContext = httpContextAccessorMock.Object.HttpContext
            }
        };
        var result = await controller.ShowInfo();
        var view_result = Assert.IsType<ViewResult>(result.Result);
        var model = Assert.IsAssignableFrom<ResponseDto<CustomerInfo>>(view_result.Model);
        Assert.NotNull(model.Data);
        Assert.Equal(expected_firstName,model.Data.FirstName);

    }

    [Fact]
    public async Task ForgotPassword_Returns_RedirectToAction()
    {
        _accountServiceMock.Setup(x => x.ForgotPassword())
            .ReturnsAsync(new ResponseDto());
        var controller = new AccountController(_accountServiceMock.Object);
        var result = await controller.ForgotPassword();
        Assert.IsType<RedirectToActionResult>(result.Result);
        Assert.IsAssignableFrom<ActionResult<ResponseDto>>(result);
    }

    [Fact]
    public async Task ResetPassword_Returns_RedirectToAction()
    {
        _accountServiceMock.Setup(x => x.ResetPassword(It.IsAny<ResetPasswordDto>()))
            .ReturnsAsync(new ResponseDto());

        var controller = new AccountController(_accountServiceMock.Object);
        var result = await controller.ResetPassword(It.IsAny<ResetPasswordDto>());
        Assert.IsType<RedirectToActionResult>(result.Result);
        Assert.IsAssignableFrom<ActionResult<ResponseDto>>(result);
    }
    
    [Fact]
    public async Task ResetPassword_Returns_View()
    {
        _accountServiceMock.Setup(x => x.ResetPassword(new ResetPasswordDto()
            {
                Password = "password"
            }))
            .ReturnsAsync(new ResponseDto());

        var controller = new AccountController(_accountServiceMock.Object);
        controller.ModelState.AddModelError("error","ModelError");
        var result = await controller.ResetPassword(It.IsAny<ResetPasswordDto>());
        var view_result = Assert.IsType<ViewResult>(result.Result);
    }

    [Fact]
    public void PasswordChangedSuccessful_Returns_View()
    {
        var controller = new AccountController(_accountServiceMock.Object);
        var result = controller.PasswordChangedSuccessful();
        Assert.IsType<ViewResult>(result);
        Assert.IsAssignableFrom<IActionResult>(result);
    }
}