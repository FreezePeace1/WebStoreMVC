using Microsoft.EntityFrameworkCore;
using WebStoreMVC.Application.Resources;
using WebStoreMVC.DAL.Context;
using WebStoreMVC.Domain.Entities;
using WebStoreMVC.Domain.Enum;
using WebStoreMVC.Dtos;
using WebStoreMVC.Services.Interfaces;

namespace WebStoreMVC.Services;

public class HomeService : IHomeService
{
    private readonly WebStoreContext _context;

    public HomeService(WebStoreContext context)
    {
        _context = context;
    }
    
    public async Task<ResponseDto<List<Product>>> Store()
    {
        var products = await _context.Products.Take(15).ToListAsync();

        return new ResponseDto<List<Product>>()
        {
            Data = products,
            SuccessMessage = SuccessMessage.ProductsAreReceived
        };
    }
}