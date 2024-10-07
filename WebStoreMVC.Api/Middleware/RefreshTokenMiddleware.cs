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
        string accessTokenExpires = context.Request.Cookies[CookieName.accessTokenExpires] ?? $"{DateTime.MinValue}";
        string accessToken = context.Request.Cookies[CookieName.accessToken] ?? string.Empty;

        if (context.Request.Cookies.ContainsKey(CookieName.accessToken) || accessToken == string.Empty ||
            accessTokenExpires != $"{DateTime.MinValue}")
        {
            string refreshToken = context.Request.Cookies[CookieName.refreshToken] ?? string.Empty;
            var expires = DateTime.Parse(accessTokenExpires);

            using var scoped = _serviceProvider.CreateScope();
            var userManager = scoped.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
            var user = await userManager.GetUserAsync(context.User);
            string userCookie = context.Request.Cookies["WebStoreMvc_Cookie"] ?? string.Empty;
            
            if (DateTime.Now > expires && refreshToken != string.Empty && user != null && userCookie != string.Empty)
            {
                context.Response.Cookies.Delete(CookieName.accessToken);

                using var scope = _serviceProvider.CreateScope();
                var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
                await authService.SetAccessTokenForBackgroundService(user);
                
                var refreshTokenFromCookies = context.Request.Cookies[CookieName.refreshToken];
                
                if (!user.RefreshToken.Equals(refreshTokenFromCookies))
                {
                    await authService.Logout();
                }
            }
        }

        await _next(context);
    }
}