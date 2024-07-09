using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using WebStoreMVC.Application.JSON;
using WebStoreMVC.Application.Resources;
using WebStoreMVC.DAL.Context;
using WebStoreMVC.Domain.Entities;
using WebStoreMVC.Domain.Enum;
using WebStoreMVC.Dtos;
using WebStoreMVC.Models;
using WebStoreMVC.Services.Interfaces;

namespace WebStoreMVC.Application.Services;

public class OrderService : IOrderService
{
    private readonly WebStoreContext _context;
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly UserManager<AppUser> _userManager;
    private readonly ILogger _logger;

    public OrderService(WebStoreContext context, IHttpContextAccessor contextAccessor, UserManager<AppUser> userManager,
        ILogger logger)
    {
        _context = context;
        _contextAccessor = contextAccessor;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<ResponseDto<CustomerInfo>> SaveCustomerInfo(CustomerInfoDto dto)
    {
        try
        {
            var user = await _userManager.FindByNameAsync(_contextAccessor.HttpContext.User.Identity.Name ?? "");

            var cartInfo = _contextAccessor.HttpContext.Session.GetJson<List<CartItem>>("Cart");

            if (cartInfo == null)
            {
                return new ResponseDto<CustomerInfo>()
                {
                    ErrorMessage = ErrorMessage.CartIsEmpty,
                    ErrorCode = (int)ErrorCode.CartIsEmpty
                };
            }

            var lastId = await _context.CustomersInfo.OrderByDescending(x => x.Id).FirstAsync();

            CustomerInfo customerInfo = new CustomerInfo();
            customerInfo.FirstName = dto.FirstName;
            customerInfo.LastName = dto.LastName;
            customerInfo.Patronymic = dto.Patronymic;
            customerInfo.PhoneNumber = dto.PhoneNumber;
            customerInfo.Address = dto.Address;
            customerInfo.City = dto.City;
            customerInfo.Id = lastId.Id + 1;
            customerInfo.AppUserId = user?.Id ?? $"Not authorized";

            await _context.CustomersInfo.AddAsync(customerInfo);

            Order userOrder = new Order()
            {
                OrderDate = DateTime.Now,
                OrderId = GenerateIdForOrder(),
                TotalPrice = (int)cartInfo.Sum(x => x.Total),
                AppUserId = user?.Id ?? $"Not authorized"
            };

            var products = new List<OrderProduct>();
            foreach (var item in cartInfo)
            {
                products.Add(new OrderProduct()
                {
                    OrderId = userOrder.OrderId,
                    ProductId = item.ProductId,
                    ProductCount = item.Quantity
                });

                var data = await _context.Products.Where(x => x.ProductId == item.ProductId).FirstOrDefaultAsync();

                if (data != null)
                {
                    await _context.Products.Where(x => x.ProductId == item.ProductId).ExecuteUpdateAsync(s => s
                        .SetProperty(p => p.ProductId, data.ProductId)
                        .SetProperty(c => c.Description, data.Description)
                        .SetProperty(p => p.Manufacturer, data.Manufacturer)
                        .SetProperty(p => p.Colour, data.Colour)
                        .SetProperty(p => p.ProductName, data.ProductName)
                        .SetProperty(p => p.Article, data.Article)
                        .SetProperty(p => p.Quantity, data.Quantity - item.Quantity)
                        .SetProperty(p => p.Hashtags, data.Hashtags)
                        .SetProperty(p => p.Images, data.Images)
                        .SetProperty(p => p.Price, data.Price));   
                }
            }
            
            await _context.OrderProducts.AddRangeAsync(products);

            await _context.Orders.AddAsync(userOrder);

            await _context.SaveChangesAsync();

            _contextAccessor.HttpContext.Session.Clear();

            return new ResponseDto<CustomerInfo>()
            {
                Data = customerInfo,
                SuccessMessage = SuccessMessage.CreatingDataIsDone
            };
        }
        catch (Exception e)
        {
            _logger.Error(e, e.Message);
            return new ResponseDto<CustomerInfo>()
            {
                ErrorMessage = ErrorMessage.ModelCreatingIsFalied,
                ErrorCode = (int)ErrorCode.CreatingModelIsFailed
            };
        }
    }

    private string GenerateIdForOrder()
    {
        return Guid.NewGuid().ToString();
    }

    public ResponseDto<List<CartItem>> ShowCartInfo()
    {
        var cartInfo = _contextAccessor.HttpContext.Session.GetJson<List<CartItem>>("Cart");

        if (cartInfo != null)
        {
            return new ResponseDto<List<CartItem>>()
            {
                Data = cartInfo,
                SuccessMessage = SuccessMessage.ShowingOrderDataIsDone
            };
        }

        return new ResponseDto<List<CartItem>>()
        {
            ErrorMessage = ErrorMessage.CartIsEmpty,
            ErrorCode = (int)ErrorCode.CartIsEmpty
        };
    }

    public async Task<ResponseDto> DeleteUserOrder(string id)
    {
        var order = await _context.Orders.FirstOrDefaultAsync(x => x.OrderId == id);

        if (order == null)
        {
            return new ResponseDto()
            {
                ErrorMessage = ErrorMessage.OrderDeletingIsFailed,
                ErrorCode = (int)ErrorCode.OrderDeletingIsFailed
            };
        }

        return new ResponseDto()
        {
            SuccessMessage = SuccessMessage.OrderIsDeletedSuccessfully
        };
    }
}