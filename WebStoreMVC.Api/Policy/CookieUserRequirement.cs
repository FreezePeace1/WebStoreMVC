using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using WebStoreMVC.Services.Interfaces;

namespace WebStoreMVC.Policy;

public class CookieUserRequirement : IAuthorizationRequirement
{
    
}

public class CookieUserRequirementHandler : AuthorizationHandler<CookieUserRequirement>
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CookieUserRequirementHandler(IHttpContextAccessor httpContextAccessor,IAuthService authService)
    {
        _httpContextAccessor = httpContextAccessor;
    }
    
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, CookieUserRequirement requirement)
    {
        var accessToken = _httpContextAccessor.HttpContext.Request.Cookies["accessToken"];
        var refreshToken = _httpContextAccessor.HttpContext.Request.Cookies["refreshToken"];

        if (!(accessToken.IsNullOrEmpty() && refreshToken.IsNullOrEmpty()))
        {
            
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}