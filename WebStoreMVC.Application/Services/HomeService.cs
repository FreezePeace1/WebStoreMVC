using Microsoft.EntityFrameworkCore;
using Serilog;
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
    private readonly ILogger _logger;

    public HomeService(WebStoreContext context, IProductsService productsService,ILogger logger)
    {
        _context = context;
        _productsService = productsService;
        _logger = logger;
    }

    public async Task<ResponseDto<List<Product>>> Store()
    {
        try
        {
            var products = await _context.Products.Take(15).ToListAsync();

            return new ResponseDto<List<Product>>()
            {
                Data = products,
                SuccessMessage = SuccessMessage.ProductsAreReceived
            };
        }
        catch (Exception e)
        {
            _logger.Error(e,e.Message);

            return new ResponseDto<List<Product>>()
            {
                ErrorMessage = ErrorMessage.ProductsAreNotFound,
                ErrorCode = (int)ErrorCode.ProductsAreNotFound
            };
        }
    }

    public async Task<ResponseDto<Product>> GetProductById(int id)
    {
        try
        {
            var currentProduct = await _context.Products.Where(x => x.ProductId == id).FirstOrDefaultAsync();

            if (currentProduct == null)
            {
                return new ResponseDto<Product>()
                {
                    ErrorMessage = ErrorMessage.ProductsAreNotFound,
                    ErrorCode = (int)ErrorCode.ProductsAreNotFound
                };
            }

            return new ResponseDto<Product>()
            {
                Data = currentProduct,
                SuccessMessage = SuccessMessage.ProductsAreReceived
            };
        }
        catch (Exception e)
        {
            _logger.Error(e, e.Message);

            return new ResponseDto<Product>()
            {
                ErrorMessage = ErrorMessage.ProductsAreNotFound,
                ErrorCode = (int)ErrorCode.ProductsAreNotFound
            };
        }
    }
}