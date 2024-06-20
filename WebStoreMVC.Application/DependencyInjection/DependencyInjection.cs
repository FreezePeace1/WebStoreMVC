using Microsoft.Extensions.DependencyInjection;
using WebStoreMVC.Application.Services;
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
        //Подключаем сервис поиска товаров
        services.AddScoped<ISearchingProductsService, SearchingProductsService>();
        //Покдлючаем сервис товаров
        services.AddScoped<IProductsService, ProductsService>();
        //Подключаем сервис корзины
        services.AddScoped<ICartService, CartService>();
        
        //Подключаем HomeService(будет доработан)
        services.AddScoped<IHomeService, HomeService>();

        services.AddScoped<Initializer>();
    }
}