using Microsoft.EntityFrameworkCore;
using WebStoreMVC.DAL.Context;
using WebStoreMVC.Domain.Entities;
using WebStoreMVC.Services.Interfaces;

namespace WebStoreMVC.Services;

public class HomeService : IHomeService
{
    private readonly WebStoreContext _context;

    public HomeService(WebStoreContext context)
    {
        _context = context;
    }
    
    public async Task<List<Product>> Store()
    {
        var products = await _context.Products.Take(15).ToListAsync();

        return products;
    }
}