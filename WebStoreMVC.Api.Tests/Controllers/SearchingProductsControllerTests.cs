using Microsoft.AspNetCore.Mvc;
using Moq;
using WebStoreMVC.Controllers;
using WebStoreMVC.Domain.Entities;
using WebStoreMVC.Dtos;
using WebStoreMVC.Services.Interfaces;
using Xunit;

namespace WebStoreMVC.Api.Controllers;

public class SearchingProductsControllerTests
{
    private readonly Mock<ISearchingProductsService> _searchingProductsServiceMock = new();

    [Fact]
    public async Task SearchingProducts_Returns_ListProductsWithView()
    {
        int expected_products_count = 3;
        int expected_lastProduct_id = 3;
        _searchingProductsServiceMock.Setup(x => x.SearchingProducts(It.IsAny<string>()))
            .ReturnsAsync(new ResponseDto<List<Product>>()
            {
                Data = new List<Product>()
                {
                    new Product()
                    {
                        ProductId = 1,
                        ProductName = "Смартфон Lg"
                    },
                    new Product()
                    {
                        ProductId = 2,
                        ProductName = "Смартфон Samsung"
                    },
                    new Product()
                    {
                        ProductId = 3,
                        ProductName = "LCD"
                    }
                }
            });

        var controller = new SearchingProductsController(_searchingProductsServiceMock.Object);
        var result = await controller.SearchingProducts();
        var view_result = Assert.IsType<ViewResult>(result.Result);
        var model = Assert.IsAssignableFrom<ResponseDto<List<Product>>>(view_result.Model);
        Assert.NotNull(model.Data);
        Assert.Equal(expected_products_count,model.Data.Count);
        Assert.Equal(expected_lastProduct_id,model.Data.Last().ProductId);
    }
    
}