using System.Reflection;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using WebStoreMVC.Areas.Admin.Controllers;
using WebStoreMVC.DAL.Context;
using WebStoreMVC.Domain.Entities;
using WebStoreMVC.Dtos;
using WebStoreMVC.Services.Interfaces;
using Xunit;

namespace WebSotoreMVC.Api.ProductsForAdmin;

public class ProductsForAdminControllerTests
{
    private readonly Mock<ISearchingProductsService> _searchingProductsServiceMock = new();
    private readonly Mock<IProductsService> _productsServiceMock = new();

    [Fact]
    public async Task GetAllProducts_Returns_ProductsListWithView()
    {
        int expected_product_count = 3;
        decimal expected_lastProduct_price = 50000;
        var products = new List<Product>()
        {
            new Product()
            {
                ProductId = 1,
                Article = 1,
                CategoryId = 1,
                CategoryName = "Смартфоны",
                Colour = "Черный",
                Description = "Описание",
                Hashtags = "Хештеги",
                Images = "source",
                Manufacturer = "Xiaomi",
                Price = 15000,
                ProductName = "Смартфон",
                Quantity = 1,
            },
            new Product()
            {
                ProductId = 2,
                Article = 2,
                CategoryId = 3,
                CategoryName = "Телевизоры",
                Colour = "Черный",
                Description = "Описание",
                Hashtags = "Хештеги",
                Images = "source",
                Manufacturer = "LG",
                Price = 30000,
                ProductName = "Телевизор",
                Quantity = 1,
            },
            new Product()
            {
                ProductId = 3,
                Article = 3,
                CategoryId = 2,
                CategoryName = "Ноутбуки",
                Colour = "Черный",
                Description = "Описание",
                Hashtags = "Хештеги",
                Images = "source",
                Manufacturer = "LG",
                Price = 50000,
                ProductName = "Ноутбук",
                Quantity = 1,
            }
        };

        _productsServiceMock.Setup(x => x.GetProductByPage(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(new ResponseDto<List<Product>>()
            {
                Data = products
            });

        _productsServiceMock.Setup(x => x.GetAllProducts())
            .ReturnsAsync(new ResponseDto<List<Product>>()
            {
                Data = products
            });

        var controller =
            new ProductsForAdminController(_productsServiceMock.Object, _searchingProductsServiceMock.Object);
        var result = await controller.GetAllProducts();
        var view_result = Assert.IsType<ViewResult>(result.Result);
        Assert.IsType<ActionResult<ResponseDto<List<Product>>>>(result);
        Assert.NotNull(result);
        var model = Assert.IsAssignableFrom<ResponseDto<List<Product>>>(view_result.Model);
        Assert.Equal(expected_product_count, model.Data.Count);
        Assert.Equal(expected_lastProduct_price, model.Data.Last().Price);
    }

    [Fact]
    public async Task GetProductById_Returns_ProductWithOkStatus()
    {
        int expected_product_id = 3;
        var product = new Product()
        {
            ProductId = 3,
        };
        _productsServiceMock.Setup(x => x.GetProductById(It.IsAny<int>()))
            .ReturnsAsync(new ResponseDto<Product>()
            {
                Data = product
            });

        var controller =
            new ProductsForAdminController(_productsServiceMock.Object, _searchingProductsServiceMock.Object);
        var result = await controller.GetProductById(expected_product_id);
        var action_result = Assert.IsType<ActionResult<Product>?>(result);
        var ok_result = Assert.IsType<OkObjectResult>(action_result.Result);
        var model = Assert.IsAssignableFrom<ResponseDto<Product>>(ok_result.Value);
        Assert.NotNull(model.Data);
        Assert.Equal(expected_product_id,model.Data.ProductId);
        Assert.NotNull(result.Result);

    }

    [Fact]
    public async Task PostProduct_Returns_RedirectToAction()
    {
        var product = new Product()
        {
            ProductId = 2
        };
        _productsServiceMock.Setup(x => x.PostProduct(It.IsAny<Product>()))
            .ReturnsAsync(new ResponseDto<Product>()
            {
                Data = product
            });

        var controller =
            new ProductsForAdminController(_productsServiceMock.Object, _searchingProductsServiceMock.Object);
        var result = await controller.PostProduct(product);

        var redirectToAction_result = Assert.IsType<RedirectToActionResult>(result.Result);
        Assert.IsAssignableFrom<ActionResult<Product>>(result);
        Assert.NotNull(result.Result);
    }

    [Fact]
    public void EditProduct_Returns_ProductWithView()
    {
        int expected_product_id = 12;
        decimal expected_product_price = 30000;
        _productsServiceMock.Setup(x => x.Edit(It.IsAny<int>()))
            .Returns(new Product()
            {
                ProductId = 12,
                Price = 30000
            });

        var controller =
            new ProductsForAdminController(_productsServiceMock.Object, _searchingProductsServiceMock.Object);
        var result = controller.EditProduct(expected_product_id);
        var view_result = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<Product>(view_result.Model);
        Assert.NotNull(model);
        Assert.Equal(expected_product_id,model.ProductId);
        Assert.Equal(expected_product_price,model.Price);
    }

    [Fact]
    public async Task CreateProduct_Returns_RedirectToAction()
    {

        var expected_product = new Product()
        {
            
        };
        _productsServiceMock.Setup(x => x.EditProduct(It.IsAny<Product>()))
            .ReturnsAsync(new ResponseDto()
            {
            });

        var controller =
            new ProductsForAdminController(_productsServiceMock.Object, _searchingProductsServiceMock.Object);
        var result = await controller.EditProduct(expected_product);
        Assert.IsType<RedirectToActionResult>(result);
        Assert.NotNull(result);
    }

    [Fact]
    public async Task DeleteProduct_Returns_RedirectToAction()
    {
        _productsServiceMock.Setup(x => x.DeleteProduct(It.IsAny<int>()))
            .ReturnsAsync(new ResponseDto());

        var controller =
            new ProductsForAdminController(_productsServiceMock.Object, _searchingProductsServiceMock.Object);
        var result = await controller.DeleteProduct(It.IsAny<int>());
        Assert.IsType<RedirectToActionResult>(result);
        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetProductByPage_Returns_OkStatusCodeWithProductList()
    {
        int page = 1;
        int expected_product_count = 3;
        string expected_lastProduct_name = "Чайник";
        _productsServiceMock.Setup(x => x.GetProductByPage(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(new ResponseDto<List<Product>>()
            {
                Data = new List<Product>()
                {
                    new Product()
                    {
                        
                    },
                    new Product()
                    {
                        
                    },
                    new Product()
                    {
                        ProductName = "Чайник"
                    },
                }
            });

        var controller =
            new ProductsForAdminController(_productsServiceMock.Object, _searchingProductsServiceMock.Object);
        var result = await controller.GetProductByPage(page, expected_product_count);
        var ok_result = Assert.IsType<OkObjectResult>(result);
        var model = Assert.IsAssignableFrom<ResponseDto<List<Product>>>(ok_result.Value);
        Assert.Equal(expected_product_count,model.Data.Count);
        Assert.Equal(expected_lastProduct_name,model.Data.Last().ProductName);
    }
    
}