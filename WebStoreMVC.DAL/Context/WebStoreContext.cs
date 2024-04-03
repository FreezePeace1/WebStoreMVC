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
}