using Microsoft.AspNetCore.Authorization;

namespace WebStoreMVC.Policy;

public class CookieAdminRequirement : IAuthorizationRequirement
{
    // Это просто маркерный интерфейс, реализация логики находится в Handler
}

public class CookieRequirementHandler : AuthorizationHandler<CookieAdminRequirement>
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CookieRequirementHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, CookieAdminRequirement adminRequirement)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        var accessToken = httpContext.Request.Cookies["accessToken"];
        var refreshToken = httpContext.Request.Cookies["refreshToken"];
        
        if (accessToken != null && refreshToken != null)
        {
            context.Succeed(adminRequirement);
        }

        return Task.CompletedTask;
    }
}