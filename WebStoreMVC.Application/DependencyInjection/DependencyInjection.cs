using Microsoft.Extensions.DependencyInjection;
using WebStoreMVC.Services;
using WebStoreMVC.Services.Data;
using WebStoreMVC.Services.Interfaces;

namespace WebStoreMVC.Application.DependencyInjection;

public static class DependencyInjection
{
    public static void AddServices(this IServiceCollection services)
    {
        //Подключаем сервис аккаунта
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<Initializer>();
    }
}