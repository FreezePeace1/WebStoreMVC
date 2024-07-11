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
    private readonly IProductsService _productsService;

    public HomeService(WebStoreContext context,IProductsService productsService)
    {
        _context = context;
        _productsService = productsService;
    }
    
    public async Task<ResponseDto<List<Product>>> Store()
    {
        var products = await _context.Products.Take(30).ToListAsync();

        return new ResponseDto<List<Product>>()
        {
            Data = products,
            SuccessMessage = SuccessMessage.ProductsAreReceived
        };
    }
}