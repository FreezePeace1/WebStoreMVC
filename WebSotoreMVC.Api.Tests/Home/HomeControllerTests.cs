using Microsoft.AspNetCore.Mvc;
using Moq;
using WebStoreMVC.Controllers;
using WebStoreMVC.Domain.Entities;
using WebStoreMVC.Services.Interfaces;
using Assert = Xunit.Assert;

namespace WebSotoreMVC.Api.Home;

[TestClass]
public class HomeControllerTests
{
    [TestMethod]
    public void Index_Returns_View()
    {
        var controller = new HomeController();
        var result = controller.Index();

        Assert.IsType<ViewResult>(result);
    }

    [TestMethod]
    public void Checkout_Returns_View()
    {
        var controller = new HomeController();
        var result = controller.Checkout();

        Assert.IsType<ViewResult>(result);
    }

    [TestMethod]
    public void Blank_Returns_View()
    {
        var controller = new HomeController();
        var result = controller.Blank();

        Assert.IsType<ViewResult>(result);
    }

    [TestMethod]
    public void Product_Returns_View()
    {
        var controller = new HomeController();
        var result = controller.Product();

        Assert.IsType<ViewResult>(result);
    }

    [TestMethod]
    public async Task Store_Returns_View_With_Products()
    {
        const int expected_id_count = 15;
        const decimal expected_price_count = 15000M;
        const string expected_productName_Of_First_Product = "Smartphone";
        const int expected_taken_items_count = 5;
        
        var products = new List<Product>
        {
            new Product()
            {
                Id = 1,
                Price = 1000M,
                ProductName = "Smartphone"
            },
            new Product()
            {
                Id = 2,
                Price = 2000M
            },
            new Product()
            {
                Id = 3,
                Price = 3000M
            },
            new Product()
            {
                Id = 4,
                Price = 4000M
            },
            new Product()
            {
                Id = 5,
                Price = 5000M
            }
        };

        var home_service_mock = new Mock<IHomeService>();
        home_service_mock.Setup(service => service.Store()).
            ReturnsAsync(products);

        var controller = new HomeController(home_service_mock.Object);
        
        var result = await controller.Store();
        var view_result = Assert.IsType<ViewResult>(result.Result);
        var model = Assert.IsAssignableFrom<List<Product>>(view_result.Model);
        
        Assert.Equal(expected_id_count,model.Sum(x => x.Id));
        Assert.Equal(expected_price_count,model.Sum(x => x.Price));
        Assert.Equal(expected_productName_Of_First_Product,model.First().ProductName);
        Assert.Equal(expected_taken_items_count,model.Count);
        
        home_service_mock.Verify(service => service.Store());
        home_service_mock.VerifyNoOtherCalls();

    }
}