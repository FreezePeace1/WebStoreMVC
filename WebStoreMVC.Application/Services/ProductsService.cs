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
        var productList = await _context.Products.AsNoTracking().ToListAsync();

        return new ResponseDto<List<Product>>()
        {
            Data = productList
        };
    }

    public async Task<ResponseDto<Product>> GetProductById(int id)
    {
        try
        {
            var product = await _context.Products.FirstOrDefaultAsync(x => x.ProductId == id);

            if (product == null)
            {
                return new ResponseDto<Product>()
                {
                    ErrorMessage = ErrorMessage.ProductsAreNotFound,
                    ErrorCode = (int)ErrorCode.ProductsAreNotFound
                };
            }
            
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

        var lastProduct = await _context.Products.OrderByDescending(x => x.ProductId)
            .FirstAsync();

        var newProduct = new Product()
        {
            Article = product.Article,
            ColorId = product.ColorId,
            Description = product.Description,
            ProductId = lastProduct.ProductId + 1,
            Images = product.Images,
            ManufacturerId = product.ManufacturerId,
            CategoryId = product.CategoryId,
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
        var product = _context.Products.FirstOrDefault(x => x.ProductId == id);

        if (product == null)
        {
            throw new NullReferenceException();
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
            await _context.Products.Where(x => x.ProductId == product.ProductId).ExecuteUpdateAsync(s => s
                .SetProperty(p => p.ProductId, product.ProductId)
                .SetProperty(c => c.Description, product.Description)
                .SetProperty(p => p.ColorId,product.ColorId)
                .SetProperty(p => p.ProductName, product.ProductName)
                .SetProperty(p => p.Article, product.Article)
                .SetProperty(p => p.Quantity, product.Quantity)
                .SetProperty(p => p.Images, product.Images)
                .SetProperty(p => p.Price, product.Price)
                .SetProperty(p => p.CategoryId,product.CategoryId)
                .SetProperty(p => p.ManufacturerId,product.ManufacturerId)
                );

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
                ProductId = id.Value
            };

            _context.Entry(product).State = EntityState.Deleted;
            await _context.SaveChangesAsync();

            /*var pr = await _context.Products.FirstOrDefaultAsync(x => x.ProductId == id);
            if (pr != null)
            {
                _context.Products.Remove(pr);
                await _context.SaveChangesAsync();
            }*/

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

    public async Task<ResponseDto<List<Product>>> GetProductByPage(int page=1, int pageSize=15)
    {
        if (page < 1)
        {
            page = 1;
        }
        
        return new ResponseDto<List<Product>>()
        {
            Data = await _context.Products.AsNoTracking().Skip((page - 1) * pageSize).Take(pageSize).ToListAsync()
        };
    }
}