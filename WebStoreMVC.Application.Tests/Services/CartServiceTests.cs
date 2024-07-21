using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using Serilog;
using WebStoreMVC.Application.JSON;
using WebStoreMVC.Application.Services;
using WebStoreMVC.DAL.Context;
using WebStoreMVC.Domain.Entities;
using WebStoreMVC.Dtos;
using WebStoreMVC.Models;
using WebStoreMVC.Models.ViewModels;
using Xunit;

namespace WebStoreMVC.Application.Tests.Services;

public class CartServiceTests
{
    private readonly Mock<IHttpContextAccessor> _httpContextMock = new();
    private readonly Mock<ILogger> _loggerMock = new();

    /*[Fact]
    public void Index_Returns_ResponseDtoWithCartViewModel()
    {

        _httpContextMock.Setup(x => x.HttpContext.Session.GetJson<List<CartItem>>(It.IsAny<string>()))
            .Returns(new List<CartItem>()
            {
                new CartItem()
                {
                },
                new CartItem()
                {
                },
                new CartItem()
                {
                },
            });

        DbContextOptionsBuilder<WebStoreContext> optsBuilder = new();
       optsBuilder.UseInMemoryDatabase(MethodBase.GetCurrentMethod().Name);
       CartService cartService;
       using (WebStoreContext dbContext = new(optsBuilder.Options))
       {
           cartService = new CartService(dbContext,_httpContextMock.Object,_loggerMock.Object);
       }

       var result = cartService.Index();
       Assert.IsType<ResponseDto<CartViewModel>>(result);

    }*/
}