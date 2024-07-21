using Microsoft.AspNetCore.Mvc;
using Moq;
using WebStoreMVC.Controllers;
using WebStoreMVC.Dtos;
using WebStoreMVC.Models;
using WebStoreMVC.Services.Interfaces;
using Xunit;

namespace WebStoreMVC.Api.Controllers;

public class SearchingProductsControllerTests
{
    private readonly Mock<ISearchingProductsService> _searchingProductsServiceMock = new();

    
    [Fact]
    public async Task SearchingProducts_Returns_ProductSearchingModelWithView()
    {
        _searchingProductsServiceMock.Setup(x => x.SearchingProducts(It.IsAny<string>(),It.IsAny<int>()))
            .ReturnsAsync(new ResponseDto<ProductSearchingModel>()
            {
                Data = new ProductSearchingModel()
                { 
                }
            });

        var controller = new SearchingProductsController(_searchingProductsServiceMock.Object);
        var result = await controller.SearchingProducts();
        var view_result = Assert.IsType<ViewResult>(result.Result);
        var model = Assert.IsAssignableFrom<ResponseDto<ProductSearchingModel>>(view_result.Model);
        Assert.NotNull(model.Data);
        Assert.Null(model.ErrorMessage);
    }
    
}