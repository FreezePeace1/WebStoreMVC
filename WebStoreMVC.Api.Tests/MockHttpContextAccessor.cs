using Microsoft.AspNetCore.Http;
using Moq;
using WebStoreMVC.Domain.Entities;

namespace WebSotoreMVC.Api;

public class MockHttpContextAccessor
{
    public void HttpContextAccessorMockAutoSetupToByPassRedirect(Mock<IHttpContextAccessor> httpContextAccessorMock)
    {
        httpContextAccessorMock.Setup(x => x.HttpContext.User.Identity.IsAuthenticated)
            .Returns(true);

        httpContextAccessorMock.Setup(x => x.HttpContext.Request.Cookies[Cookie.refreshToken])
            .Returns("token");
        httpContextAccessorMock.Setup(x => x.HttpContext.Request.Cookies[Cookie.accessToken])
            .Returns("token");
    }

    public void HttpContextAccessorMockAutoSetupForRedirect(Mock<IHttpContextAccessor> httpContextAccessorMock)
    {
        httpContextAccessorMock.Setup(x => x.HttpContext.User.Identity.IsAuthenticated)
            .Returns(false);
        httpContextAccessorMock.Setup(x => x.HttpContext.Request.Cookies[Cookie.refreshToken]);
        httpContextAccessorMock.Setup(x => x.HttpContext.Request.Cookies[Cookie.accessToken]);
    }
}