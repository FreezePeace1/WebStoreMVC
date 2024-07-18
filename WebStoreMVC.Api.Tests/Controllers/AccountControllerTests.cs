using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WebStoreMVC.Controllers;
using WebStoreMVC.Dtos;
using WebStoreMVC.Models;
using WebStoreMVC.Services.Interfaces;
using Xunit;

namespace WebSotoreMVC.Api.Controllers;

public class AccountControllerTests
{
    public readonly Mock<IAccountService> _accountServiceMock = new();

    /*[Fact]
    public async Task Index_Returns_ListProductOrderWithView()
    {
        //TODO - Mock the cookies
        _accountServiceMock.Setup(x => x.Index())
            .ReturnsAsync(new ResponseDto<List<ProductOrderModel>>()
            {
                Data = new List<ProductOrderModel>()
                {
                    new ProductOrderModel()
                    {
                        
                    }
                }
            });

        var controller = new AccountController(_accountServiceMock.Object)
        {
            
        };
        var result = await controller.Index();
        Assert.IsType<ViewResult>(result.Result);
    }*/
}