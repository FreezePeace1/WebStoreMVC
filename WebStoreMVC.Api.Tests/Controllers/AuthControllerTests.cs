using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Moq;
using WebStoreMVC.Controllers;
using WebStoreMVC.Dtos;
using WebStoreMVC.Services.Interfaces;
using Xunit;

namespace WebSotoreMVC.Api.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IAuthService> _authServiceMock = new();
    private readonly MockHttpContextAccessor _httpContextAccessorMock = new();

    [Fact]
    public async Task SeedingRoles_Returns_OkStatusCodeWithResponseDto()
    {
        string expected_successMessage = "success message";
        _authServiceMock.Setup(x => x.SeedRoles())
            .ReturnsAsync(new ResponseDto()
            {
                SuccessMessage = "success message"
            });

        var controller = new AuthController(_authServiceMock.Object);
        var result = await controller.SeedingRoles();
        var ok_result = Assert.IsType<OkObjectResult>(result.Result);
        var model = Assert.IsAssignableFrom<ResponseDto>(ok_result.Value);
        Assert.NotNull(model.SuccessMessage);
        Assert.Equal(expected_successMessage,model.SuccessMessage);
    }

    [Fact]
    public async Task Registration_Returns_RedirectToAction()
    {
        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _httpContextAccessorMock.HttpContextAccessorMockAutoSetupForRedirect(httpContextAccessorMock);
        _authServiceMock.Setup(x => x.Register(It.IsAny<RegisterDto>()))
            .ReturnsAsync(new ResponseDto());
        var controller = new AuthController(_authServiceMock.Object)
        {
            ControllerContext =
            {
                HttpContext = httpContextAccessorMock.Object.HttpContext
            }
        };

        var result = await controller.Registration(new RegisterDto());
        Assert.IsType<RedirectToActionResult>(result);
        Assert.IsAssignableFrom<ActionResult>(result);
    }
    
    [Fact]
    public async Task Registration_Returns_RedirectToActionWithSucceedService()
    {
        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _httpContextAccessorMock.HttpContextAccessorMockAutoSetupToByPassRedirect(httpContextAccessorMock);
        _authServiceMock.Setup(x => x.Register(It.IsAny<RegisterDto>()))
            .ReturnsAsync(new ResponseDto());
        var controller = new AuthController(_authServiceMock.Object)
        {
            ControllerContext =
            {
                HttpContext = httpContextAccessorMock.Object.HttpContext
            }
        };

        var result = await controller.Registration(new RegisterDto());
        Assert.IsType<RedirectToActionResult>(result);
        Assert.IsAssignableFrom<ActionResult>(result);
    }
    
    [Fact]
    public void Registration_Returns_View()
    {
        var controller = new AuthController(_authServiceMock.Object);

        var result = controller.Registration();
        var view_result = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<RegisterDto>(view_result.Model);
        Assert.NotNull(model);
    }

    [Fact]
    public void Login_Returns_View()
    {
        var controller = new AuthController(_authServiceMock.Object);
        var result = controller.Login();
        var view_result = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<LoginDto>(view_result.Model);
        Assert.NotNull(model);

    }

    [Fact]
    public async Task Login_Returns_RedirectToAction()
    {
        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _httpContextAccessorMock.HttpContextAccessorMockAutoSetupForRedirect(httpContextAccessorMock);
        _authServiceMock.Setup(x => x.Login(It.IsAny<LoginDto>()))
            .ReturnsAsync(new ResponseDto());

        var controller = new AuthController(_authServiceMock.Object)
        {
            ControllerContext =
            {
                HttpContext = httpContextAccessorMock.Object.HttpContext
            }
        };
        var result = await controller.Login(It.IsAny<LoginDto>());

        Assert.IsType<RedirectToActionResult>(result);
    }
    
    [Fact]
    public async Task Login_Returns_RedirectToActionWithSucceedService()
    {
        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _httpContextAccessorMock.HttpContextAccessorMockAutoSetupToByPassRedirect(httpContextAccessorMock);
        _authServiceMock.Setup(x => x.Login(It.IsAny<LoginDto>()))
            .ReturnsAsync(new ResponseDto());

        var controller = new AuthController(_authServiceMock.Object)
        {
            ControllerContext =
            {
                HttpContext = httpContextAccessorMock.Object.HttpContext
            }
        };
        var result = await controller.Login(It.IsAny<LoginDto>());

        Assert.IsType<RedirectToActionResult>(result);
    }

    [Fact]
    public async Task FromUserToAdmin_Returns_StatusCodeOkWithResponseDto()
    {
        string expected_successMessage = "success message";
        _authServiceMock.Setup(x => x.FromUserToAdmin(It.IsAny<UpdateDto>()))
            .ReturnsAsync(new ResponseDto()
            {
                SuccessMessage = "success message"
            });

        var controller = new AuthController(_authServiceMock.Object);
        var result = await controller.FromUserToAdmin(It.IsAny<UpdateDto>());

        var ok_result = Assert.IsType<OkObjectResult>(result.Result);
        var model = Assert.IsAssignableFrom<ResponseDto>(ok_result.Value);
        Assert.Equal(expected_successMessage,model.SuccessMessage);
        Assert.Null(model.ErrorMessage);
    }

    [Fact]
    public async Task Logout_Returns_RedirectToAction()
    {
        _authServiceMock.Setup(x => x.Logout())
            .ReturnsAsync(new ResponseDto());

        var controller = new AuthController(_authServiceMock.Object);
        var result = await controller.Logout();
        Assert.IsType<RedirectToActionResult>(result);
        Assert.IsAssignableFrom<ActionResult>(result);
    }
    
    [Fact]
    public async Task FromAdminToUser_Returns_StatusCodeOkWithResponseDto()
    {
        string expected_successMessage = "success message";
        _authServiceMock.Setup(x => x.FromAdminToUser(It.IsAny<UpdateDto>()))
            .ReturnsAsync(new ResponseDto()
            {
                SuccessMessage = "success message"
            });

        var controller = new AuthController(_authServiceMock.Object);
        var result = await controller.FromAdminToUser(new UpdateDto());

        var ok_result = Assert.IsType<OkObjectResult>(result.Result);
        var model = Assert.IsAssignableFrom<ResponseDto>(ok_result.Value);
        Assert.Equal(expected_successMessage,model.SuccessMessage);
        Assert.Null(model.ErrorMessage);
    }

    [Fact]
    public async Task VerifyAccount_Returns_RedirectToAction()
    {
        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _httpContextAccessorMock.HttpContextAccessorMockAutoSetupForRedirect(httpContextAccessorMock);
        _authServiceMock.Setup(x => x.VerifyAccount(It.IsAny<string>()))
            .ReturnsAsync(new ResponseDto());

        var controller = new AuthController(_authServiceMock.Object)
        {
            ControllerContext =
            {
                HttpContext = httpContextAccessorMock.Object.HttpContext
            }
        };
        var result = await controller.VerifyAccount(new VerifyAccountDto());
        Assert.IsType<RedirectToActionResult>(result.Result);
        Assert.IsAssignableFrom<ActionResult<ResponseDto<VerifyAccountDto>>>(result);
    }

    [Fact]
    public async Task VerifyAccount_Returns_View()
    {
        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _httpContextAccessorMock.HttpContextAccessorMockAutoSetupForRedirect(httpContextAccessorMock);
        _authServiceMock.Setup(x => x.VerifyAccount(It.IsAny<string>()))
            .ReturnsAsync(new ResponseDto());

        var controller = new AuthController(_authServiceMock.Object)
        {
            ControllerContext =
            {
                HttpContext = httpContextAccessorMock.Object.HttpContext
            }
        };
        controller.ModelState.AddModelError("error", "ModelError");
        var result = await controller.VerifyAccount(new VerifyAccountDto());
        Assert.IsType<ViewResult>(result.Result);
        Assert.IsAssignableFrom<ActionResult<ResponseDto<VerifyAccountDto>>>(result);
    }

    [Fact]
    public async Task ForgotPassword_Returns_RedirectToAction()
    {
        _authServiceMock.Setup(x => x.ForgotPassword(It.IsAny<string>()))
            .ReturnsAsync(new ResponseDto());

        var controller = new AuthController(_authServiceMock.Object);
        var result = await controller.ForgotPassword(new ForgotPasswordDto());

        Assert.IsType<RedirectToActionResult>(result);
        Assert.IsAssignableFrom<IActionResult>(result);
    }

    [Fact]
    public async Task ForgotPassword_Returns_ViewWithResetPasswordDto()
    {
        string expected_email = "email";
        _authServiceMock.Setup(x => x.ForgotPassword(It.IsAny<string>()))
            .ReturnsAsync(new ResponseDto());

        var controller = new AuthController(_authServiceMock.Object);
        controller.ModelState.AddModelError("error","ModelError");
        var result = await controller.ForgotPassword(new ForgotPasswordDto()
        {
            Email = "email"
        });

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.IsAssignableFrom<IActionResult>(result);
        var model = Assert.IsAssignableFrom<ForgotPasswordDto>(viewResult.Model);
        Assert.NotNull(model);
        Assert.Equal(expected_email,model.Email);
    }

    [Fact]
    public async Task ResetPassword_Returns_RedirectToAction()
    {
        _authServiceMock.Setup(x => x.ResetPassword(It.IsAny<ResetPasswordDto>()))
            .ReturnsAsync(new ResponseDto());

        var controller = new AuthController(_authServiceMock.Object);
        var result = await controller.ResetPassword(new ResetPasswordDto());
        Assert.IsType<RedirectToActionResult>(result);
        Assert.IsAssignableFrom<IActionResult>(result);
    }

    [Fact]
    public async Task ResetPassword_Returns_ViewWithResetPasswordDto()
    {
        string expected_tokenForPasswordReseting = "token";
        _authServiceMock.Setup(x => x.ResetPassword(It.IsAny<ResetPasswordDto>()))
            .ReturnsAsync(new ResponseDto());

        var controller = new AuthController(_authServiceMock.Object);
        controller.ModelState.AddModelError("error","ModelError");
        var result = await controller.ResetPassword(new ResetPasswordDto()
        {
            TokenForPasswordReseting = "token"
        });
        var view_result = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<ResetPasswordDto>(view_result.Model);
        Assert.NotNull(model);
        Assert.Equal(expected_tokenForPasswordReseting,model.TokenForPasswordReseting);
    }
}