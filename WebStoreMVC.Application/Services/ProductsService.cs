using Microsoft.EntityFrameworkCore;
using Serilog;
using WebStoreMVC.Application.Resources;
using WebStoreMVC.DAL.Context;
using WebStoreMVC.Domain.Entities;
using WebStoreMVC.Domain.Enum;
using WebStoreMVC.Dtos;
using WebStoreMVC.Services.Interfaces;

namespace WebStoreMVC.Services;

public class ProductsService : IProductsService
{
    private readonly WebStoreContext _context;
    private readonly ILogger _logger;

    public ProductsService(WebStoreContext context, ILogger logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ResponseDto<List<Product>>> GetAllProducts()
    {
        var productList = await _context.Products.AsNoTracking().Take(3000).ToListAsync();

        return new ResponseDto<List<Product>>()
        {
            Data = productList
        };
    }

    public async Task<ResponseDto<Product>> GetProductById(int id)
    {
        try
        {
            var product = await _context.Products.FirstOrDefaultAsync(x => x.Id == id);

            return new ResponseDto<Product>()
            {
                Data = product
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

    public async Task<ResponseDto<Product>> PostProduct(Product product)
    {
        if (product is null)
        {
            throw new ArgumentNullException(nameof(product));
        }

        if (await _context.Products.ContainsAsync(product))
        {
            return new ResponseDto<Product>()
            {
                ErrorMessage = ErrorMessage.ProductAlreadyExists,
                ErrorCode = (int)ErrorCode.ProductAlreadyExists
            };
        }

        var lastProduct = await _context.Products.OrderByDescending(x => x.Id)
            .FirstAsync();

        var newProduct = new Product()
        {
            Article = product.Article,
            Colour = product.Colour,
            Description = product.Description,
            Hashtags = product.Hashtags,
            Id = lastProduct.Id + 1,
            Images = product.Images,
            Manufacturer = product.Manufacturer,
            Price = product.Price,
            ProductName = product.ProductName,
            Quantity = product.Quantity
        };

        await _context.AddAsync(newProduct);
        await _context.SaveChangesAsync();

        return new ResponseDto<Product>()
        {
            SuccessMessage = SuccessMessage.ProductsAreReceived
        };
    }

    public Product Edit(int id)
    {
        var product = _context.Products.FirstOrDefault(x => x.Id == id);

        if (id < 0)
        {
            throw new ArgumentException(nameof(product));
        }

        return product;
    }

    public async Task<ResponseDto> EditProduct(Product product)
    {
        if (product == null)
        {
            return new ResponseDto()
            {
                ErrorMessage = ErrorMessage.ProductsAreNotFound,
                ErrorCode = (int)ErrorCode.ProductsAreNotFound
            };
        }

        try
        {
            await _context.Products.Where(x => x.Id == product.Id).ExecuteUpdateAsync(s => s
                .SetProperty(p => p.Id, product.Id)
                .SetProperty(c => c.Description, product.Description)
                .SetProperty(p => p.Manufacturer, product.Manufacturer)
                .SetProperty(p => p.Colour, product.Colour)
                .SetProperty(p => p.ProductName, product.ProductName)
                .SetProperty(p => p.Article, product.Article)
                .SetProperty(p => p.Quantity, product.Quantity)
                .SetProperty(p => p.Hashtags, product.Hashtags)
                .SetProperty(p => p.Images, product.Images)
                .SetProperty(p => p.Price, product.Price));

            await _context.SaveChangesAsync();

            return new ResponseDto()
            {
                SuccessMessage = SuccessMessage.ProductsAreReceived
            };
        }
        catch (Exception e)
        {
            _logger.Error(e, e.Message);

            return new ResponseDto()
            {
                ErrorMessage = ErrorMessage.InternalServerError,
                ErrorCode = (int)ErrorCode.InternalServerError
            };
        }
    }

    public async Task<ResponseDto> DeleteProduct(int? id)
    {
        if (id != null)
        {
            Product product = new Product()
            {
                Id = id.Value
            };

            _context.Entry(product).State = EntityState.Deleted;
            await _context.SaveChangesAsync();

            return new ResponseDto()
            {
                SuccessMessage = SuccessMessage.ProductsAreReceived
            };
        }

        return new ResponseDto()
        {
            ErrorMessage = ErrorMessage.ProductsAreNotFound,
            ErrorCode = (int)ErrorCode.ProductsAreNotFound
        };
    }

    public async Task<ResponseDto<List<Product>>> GetProductByPage(int page, int pageSize)
    {
        return new ResponseDto<List<Product>>()
        {
            Data = await _context.Products.AsNoTracking().Skip((page - 1) * pageSize).Take(pageSize).ToListAsync()
        };
    }
}