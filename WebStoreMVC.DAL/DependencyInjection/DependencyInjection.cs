using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WebStoreMVC.DAL.Context;

namespace WebStoreMVC.DAL.DependencyInjection;

public static class DependencyInjection
{
    public static void AddDataAccessLayer(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<WebStoreContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DockerConnection"))); // ConnectionString in user secrets!
        //Example: Host=postgres_db;Database=database_name;Username=postgres;Password=password
    }
}