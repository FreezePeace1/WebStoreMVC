using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebStoreMVC.Domain.Entities;

namespace WebStoreMVC.DAL.Context;

public class WebStoreContext : IdentityDbContext<AppUser, IdentityRole, string>
{
    public WebStoreContext(DbContextOptions<WebStoreContext> options) : base(options)
    {
        Database.EnsureCreated();
    }

    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<CustomerInfo> CustomersInfo { get; set; }
    public DbSet<OrderProduct> OrderProducts { get; set; }
    public DbSet<UserReview> UserReviews { get; set; }
    
    public DbSet<Color> Colors { get; set; }
    
    public DbSet<Category> Categories { get; set; }
    
    public DbSet<Manufacturer> Manufacturers { get; set; }
 }