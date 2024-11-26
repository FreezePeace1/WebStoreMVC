using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MimeKit;
using MimeKit.Text;
using Serilog;
using Stripe.Checkout;
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
    private readonly IConfiguration _configuration;
    private readonly ILogger _logger;

    public OrderService(WebStoreContext context, IHttpContextAccessor contextAccessor, UserManager<AppUser> userManager,
        IConfiguration configuration, ILogger logger)
    {
        _context = context;
        _contextAccessor = contextAccessor;
        _userManager = userManager;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<ResponseDto> SaveUserOrder()
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

            Order userOrder = new Order()
            {
                OrderDate = DateTime.Now,
                OrderId = GenerateId(),
                TotalPrice = (int)cartInfo.Sum(x => x.Total),
                AppUserId = user?.Id ?? $"",
                OrderStatus = "Заказ собирается"
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
                        .SetProperty(p => p.ColorId,data.ColorId)
                        .SetProperty(p => p.CategoryId,data.CategoryId)
                        .SetProperty(p => p.ManufacturerId,data.ManufacturerId)
                        .SetProperty(p => p.ProductName, data.ProductName)
                        .SetProperty(p => p.Article, data.Article)
                        .SetProperty(p => p.Quantity, data.Quantity - item.Quantity)
                        .SetProperty(p => p.Images, data.Images)
                        .SetProperty(p => p.Price, data.Price));
                }
            }

            await _context.OrderProducts.AddRangeAsync(products);

            await _context.Orders.AddAsync(userOrder);

            await _context.SaveChangesAsync();

            _contextAccessor.HttpContext.Session.Clear();

            return new ResponseDto()
            {
                SuccessMessage = SuccessMessage.SavingOrderIsDone
            };
        }
        catch (Exception e)
        {
            _logger.Error(e, e.Message);
            return new ResponseDto()
            {
                ErrorMessage = ErrorMessage.GettingOrderDataIsFailed,
                ErrorCode = (int)ErrorCode.GettingOrderDataIsFailed
            };
        }
    }

    public async Task<ResponseDto<Session>> StripePayment()
    {
        try
        {
            //Stripe payment system
            string domain = _configuration.GetSection("Domain").Value;

            var opts = new SessionCreateOptions()
            {
                SuccessUrl = domain + $"Order/OrderConfirmation",
                CancelUrl = domain + $"ShowCartInfo",
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment"
            };

            string email = string.Empty;
            if (_contextAccessor.HttpContext.User.Identity.IsAuthenticated)
            {
                var user = await _userManager.FindByNameAsync(_contextAccessor.HttpContext.User.Identity.Name ?? "");

                if (user != null)
                {
                    email = user.Email;
                    opts.CustomerEmail = email;
                }
            }

            var cartInfo = _contextAccessor.HttpContext.Session.GetJson<List<CartItem>>("Cart");

            if (cartInfo == null)
            {
                return new ResponseDto<Session>()
                {
                    ErrorMessage = ErrorMessage.CartIsEmpty,
                    ErrorCode = (int)ErrorCode.CartIsEmpty
                };
            }

            foreach (var item in cartInfo)
            {
                var sessionListItem = new SessionLineItemOptions()
                {
                    PriceData = new SessionLineItemPriceDataOptions()
                    {
                        UnitAmount = decimal.ToInt64(item.Quantity * item.Price) * 100,
                        Currency = "rub",
                        ProductData = new SessionLineItemPriceDataProductDataOptions()
                        {
                            Name = item.ProductName
                        }
                    },
                    Quantity = item.Quantity
                };

                opts.LineItems.Add(sessionListItem);
            }

            var service = new SessionService();
            var session = await service.CreateAsync(opts);
            
            _contextAccessor.HttpContext.Response.Headers.Append("Location", session.Url);

            return new ResponseDto<Session>()
            {
                SuccessMessage = session.Id,
                Data = session
            };
        }
        catch (Exception e)
        {
            _logger.Error(e, e.Message);
            return new ResponseDto<Session>()
            {
                ErrorMessage = ErrorMessage.TransactionIsFalied,
                ErrorCode = (int)ErrorCode.TransactionIsFailed
            };
        }
    }

    public async Task<ResponseDto<CustomerInfo>> SaveCustomerInfo(CustomerInfoDto dto)
    {
        try
        {
            var user = await _userManager.FindByNameAsync(_contextAccessor.HttpContext.User.Identity.Name ?? "");

            CustomerInfo customerInfo = new CustomerInfo();
            customerInfo.FirstName = dto.FirstName;
            customerInfo.LastName = dto.LastName;
            customerInfo.Patronymic = dto.Patronymic;
            customerInfo.PhoneNumber = dto.PhoneNumber;
            customerInfo.Address = dto.Address;
            customerInfo.City = dto.City;
            customerInfo.AppUserId = user?.Id ?? $"";
            customerInfo.UserEmail = dto.UserEmail;

            await _context.CustomersInfo.AddAsync(customerInfo);

            await _context.SaveChangesAsync();

            return new ResponseDto<CustomerInfo>()
            {
                Data = customerInfo,
                SuccessMessage = SuccessMessage.DataIsRecieved
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

    private string GenerateId()
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

    public async Task<ResponseDto<Order>> GetLastOrder()
    {
        var order = await _context.Orders.OrderByDescending(x => x.OrderDate).FirstOrDefaultAsync();

        return new ResponseDto<Order>()
        {
            Data = order
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

        _context.Entry(order).State = EntityState.Deleted;

        await _context.SaveChangesAsync();

        return new ResponseDto()
        {
            SuccessMessage = SuccessMessage.OrderIsDeletedSuccessfully
        };
    }

    public async Task<ResponseDto<List<ProductOrderModel>>> FindUserOrder(string id)
    {
        var userOrder = await _context.Orders.FirstOrDefaultAsync(x => x.OrderId == id);

        if (userOrder == null)
        {
            return new ResponseDto<List<ProductOrderModel>>()
            {
                ErrorMessage = ErrorMessage.OrderIsNotFound,
                ErrorCode = (int)ErrorCode.OrderIsNotFound
            };
        }
        
        var productOrder = (from o in _context.Orders
            join op in _context.OrderProducts on o.OrderId equals op.OrderId
            join p in _context.Products on op.ProductId equals p.ProductId
            join c in _context.CustomersInfo on o.AppUserId equals c.AppUserId
            join col in _context.Colors on p.ColorId equals col.Id
            where op.OrderId == userOrder.OrderId
            select new
            {
                OrderId = o.OrderId,
                TotalPrice = o.TotalPrice,
                OrderDate = o.OrderDate,
                ProductCount = op.ProductCount,
                ProductName = p.ProductName,
                Price = p.Price,
                Images = p.Images,
                Address = c.Address,
                City = c.City,
                Color = col.ColorName
            });


        var orders = new List<ProductOrderModel>();
        foreach (var item in productOrder)
        {
            var order = new ProductOrderModel();
            
            order.ProductName = item.ProductName;
            order.Colour = item.Color;
            order.Images = item.Images;
            order.OrderDate = item.OrderDate;
            order.Price = item.Price;
            order.Quantity = item.ProductCount;
            order.TotalPrice = item.TotalPrice;
            order.Address = item.Address;
            order.City = item.City;
            order.OrderId = item.OrderId;
            orders.Add(order);
            
        }

        return new ResponseDto<List<ProductOrderModel>>()
        {
            Data = orders,
            SuccessMessage = SuccessMessage.OrderIsFound
        };
    }

    public async Task<ResponseDto> SendOrderToUserEmail(OrderWithUserMail orderWithUserMail)
    {
        try
        {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_configuration.GetSection("EmailUsername").Value));
            email.To.Add(MailboxAddress.Parse(orderWithUserMail.UserEmail));
            email.Subject = "Message from ElectroStore";
            email.Body = new TextPart(TextFormat.Html)
            {
                Text = $"Поздравляем с покупкой! Номер вашего заказа: {orderWithUserMail.Order.OrderId} Дата оформления заказа: {orderWithUserMail.Order.OrderDate}" +
                       $"\nОбщая сумма заказа: {orderWithUserMail.Order.TotalPrice}" +
                       $"\nДля отслеживания заказа зайдите на сайт и используйте функцию \"Найти заказ\""
            };

            using var smtp = new SmtpClient();
        
            await smtp.ConnectAsync(_configuration.GetSection("EmailHost").Value, 587,SecureSocketOptions.StartTls);
        
            await smtp.AuthenticateAsync(_configuration.GetSection("EmailUsername").Value, _configuration.GetSection("EmailPassword").Value);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);

            return new ResponseDto()
            {
                SuccessMessage = SuccessMessage.EmailSuccess
            };
        }
        catch (Exception e)
        {
            _logger.Error(e,e.Message);
            return new ResponseDto()
            {
                ErrorMessage = ErrorMessage.EmailFailure,
                ErrorCode = (int)ErrorCode.EmailFailure
            };
        }
    }
    
    public async Task<ResponseDto> SendUserInfoAfterSuccessfulOrder(RegisterDto dto)
    {
        try
        {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_configuration.GetSection("EmailUsername").Value));
            email.To.Add(MailboxAddress.Parse(dto.Email));
            email.Subject = "Message from ElectroStore";
            email.Body = new TextPart(TextFormat.Html)
            {
                Text = $"Вы автоматически зарегестировались после покупки, поздравляем!\n" +
                       $"Данные вашего аккаунта для входа:\n" +
                       $"Логин: {dto.Username}\n" +
                       $"Пароль: {dto.Password} \n" +
                       $"В личном аккаунте вы можете отслеживать статус вашего заказа!"
            };

            using var smtp = new SmtpClient();
        
            await smtp.ConnectAsync(_configuration.GetSection("EmailHost").Value, 587,SecureSocketOptions.StartTls);
        
            await smtp.AuthenticateAsync(_configuration.GetSection("EmailUsername").Value, _configuration.GetSection("EmailPassword").Value);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);

            return new ResponseDto()
            {
                SuccessMessage = SuccessMessage.EmailSuccess
            };
        }
        catch (Exception e)
        {
            _logger.Error(e,e.Message);
            return new ResponseDto()
            {
                ErrorMessage = ErrorMessage.EmailFailure,
                ErrorCode = (int)ErrorCode.EmailFailure
            };
        }
    }
}