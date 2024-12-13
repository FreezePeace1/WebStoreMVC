using System.Globalization;
using Microsoft.AspNetCore.Identity;
using WebStoreMVC.Domain.Entities;
using WebStoreMVC.Services.Interfaces;

namespace WebStoreMVC.Middleware;

public class RefreshTokenMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IServiceProvider _serviceProvider;


    public RefreshTokenMiddleware(RequestDelegate next, IServiceProvider serviceProvider)
    {
        _next = next;
        _serviceProvider = serviceProvider;
    }

    public async Task Invoke(HttpContext context)
    {
        //Время будет больше для избежания лишнего обновления access токена
        var accessTokenExpires = DateTime.Now.AddMinutes(Cookie.accessTokenExpiresTime + 1);
        string accessToken = context.Request.Cookies[Cookie.accessToken] ?? string.Empty;

        if (accessToken == "")
        {
            accessTokenExpires = DateTime.Now;
        }
        
        string refreshToken = context.Request.Cookies[Cookie.refreshToken] ?? string.Empty;
        string userCookie = context.Request.Cookies[Cookie.userCookie] ?? string.Empty;
        if (!string.IsNullOrEmpty(refreshToken) && !string.IsNullOrEmpty(userCookie) && accessTokenExpires != DateTime.Now)
        {
            var expires = accessTokenExpires;
            
            using var scoped = _serviceProvider.CreateScope();
            var userManager = scoped.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
            var user = await userManager.GetUserAsync(context.User);

            using var scope = _serviceProvider.CreateScope();
            var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();

            if (DateTime.Now > expires && user != null)
            {
                context.Response.Cookies.Delete(Cookie.accessToken);

                await authService.SetAccessTokenForMiddleware(user);
            }

            var refreshTokenFromCookies = context.Request.Cookies[Cookie.refreshToken];

            if (!user.RefreshToken.Equals(refreshTokenFromCookies))
            {
                await authService.Logout();
            }
        }

        await _next(context);
    }
}