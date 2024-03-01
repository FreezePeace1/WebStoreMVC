using Microsoft.EntityFrameworkCore;
using WebStoreMVC.Domain.Entities;

namespace WebStoreMVC.DAL.Context;

public class WebStoreContext : DbContext
{
    public WebStoreContext(DbContextOptions<WebStoreContext> options) : base(options) { }

    public DbSet<Product> Products { get; set; }
}