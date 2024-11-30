using System.Security.Cryptography;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MimeKit;
using MimeKit.Text;
using Serilog;
using WebStoreMVC.Application.Resources;
using WebStoreMVC.DAL.Context;
using WebStoreMVC.Domain.Entities;
using WebStoreMVC.Domain.Enum;
using WebStoreMVC.Dtos;
using WebStoreMVC.Models;
using WebStoreMVC.Services.Interfaces;

namespace WebStoreMVC.Application.Services;

public class AccountService : IAccountService
{
    private readonly WebStoreContext _context;
    private readonly UserManager<AppUser> _userManager;
    private readonly IHttpContextAccessor _httpContext;
    private readonly IConfiguration _configuration;
    private readonly ILogger _logger;

    public AccountService(WebStoreContext context, UserManager<AppUser> userManager, IHttpContextAccessor httpContext,
        IConfiguration configuration,ILogger logger)
    {
        _context = context;
        _userManager = userManager;
        _httpContext = httpContext;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<ResponseDto<List<ProductOrderModel>>> Index()
    {
        try
        {
            var userName = _httpContext.HttpContext.User.Identity.Name;
            var user = await _userManager.FindByNameAsync(userName);
            var customerInfo = await _context.CustomersInfo
                .Where(x => x.AppUserId == user.Id || x.UserEmail == user.Email).ToListAsync();
            
            var productOrder = (from o in _context.Orders
                join op in _context.OrderProducts on o.OrderId equals op.OrderId
                join p in _context.Products on op.ProductId equals p.ProductId
                join c in _context.Colors on p.ColorId equals c.Id
                where o.AppUserId == user.Id
                select new
                {
                    OrderId = o.OrderId,
                    TotalPrice = o.TotalPrice,
                    OrderDate = o.OrderDate,
                    ProductCount = op.ProductCount,
                    ProductName = p.ProductName,
                    Price = p.Price,
                    Images = p.Images,
                    Color = c.ColorName,
                    ProductId = p.ProductId
                });

            if (productOrder.Distinct().Any())
            {
                var orders = new List<ProductOrderModel>();
                    foreach (var info in customerInfo)
                    {
                        foreach (var item in productOrder)
                        {
                            var order = new ProductOrderModel();
                            order.ProductName = item.ProductName;
                            order.Images = item.Images;
                            order.OrderDate = item.OrderDate;
                            order.Price = item.Price;
                            order.Quantity = item.ProductCount;
                            order.TotalPrice = item.TotalPrice;
                            order.OrderId = item.OrderId;
                            order.City = info.City;
                            order.Address = info.Address;
                            order.Colour = item.Color;
                            order.ProductId = item.ProductId;

                            orders.Add(order);   
                        }
                    }

                return new ResponseDto<List<ProductOrderModel>>()
                {
                    Data = orders
                };
            }

            return new ResponseDto<List<ProductOrderModel>>()
            {
                ErrorMessage = ErrorMessage.ProductsAreNotFound,
                ErrorCode = (int)ErrorCode.ProductsNotFound
            };
        }
        catch (Exception e)
        {
            _logger.Error(e, e.Message);
            return new ResponseDto<List<ProductOrderModel>>()
            {
                ErrorMessage = ErrorMessage.FailureToShowOrderInfo,
                ErrorCode = (int)ErrorCode.FailureToShowOrderInfo
            };
        }
    }

    public async Task<ResponseDto<CustomerInfoDto>> ChangeInfo(CustomerInfoDto dto)
    {
        try
        {
            var userName = _httpContext.HttpContext.User.Identity.Name;
            var user = await _userManager.FindByNameAsync(userName);

            var userInfo = await _context.CustomersInfo.Where(x => x.AppUserId == user.Id).AnyAsync();
            if (userInfo)
            {
                await _context.CustomersInfo.Where(x => x.AppUserId == user.Id)
                    .ExecuteUpdateAsync(x => x
                        .SetProperty(i => i.FirstName, dto.FirstName)
                        .SetProperty(i => i.LastName, dto.LastName)
                        .SetProperty(i => i.Patronymic, dto.Patronymic)
                        .SetProperty(i => i.PhoneNumber, dto.PhoneNumber)
                        .SetProperty(i => i.Address, dto.Address)
                        .SetProperty(i => i.City, dto.City)
                    );

                await _context.SaveChangesAsync();

                return new ResponseDto<CustomerInfoDto>()
                {
                    SuccessMessage = SuccessMessage.UserInfoChanged,
                    Data = dto
                };
            }

            return new ResponseDto<CustomerInfoDto>()
            {
                ErrorMessage = ErrorMessage.UserDoesNotExist,
                ErrorCode = (int)ErrorCode.UserDoesNotExist
            };
        }
        catch (Exception e)
        {
            _logger.Error(e, e.Message);
            return new ResponseDto<CustomerInfoDto>()
            {
                ErrorMessage = ErrorMessage.UserDoesNotExist,
                ErrorCode = (int)ErrorCode.UserDoesNotExist
            };
        }
    }

    public async Task<ResponseDto<CustomerInfo>> ShowInfo()
    {
        try
        {
            var userName = _httpContext.HttpContext.User.Identity.Name;
            var user = await _userManager.FindByNameAsync(userName);
            var userInfo = await _context.CustomersInfo.Where(x => x.AppUserId == user.Id).FirstOrDefaultAsync();
            if (userInfo != null)
            {
                return new ResponseDto<CustomerInfo>()
                {
                    Data = userInfo,
                    SuccessMessage = SuccessMessage.DataIsRecieved
                };
            }

            return new ResponseDto<CustomerInfo>()
            {
                SuccessMessage = SuccessMessage.UserInfoIsNotFound
            };
        }
        catch (Exception e)
        {
            _logger.Error(e, e.Message);
            return new ResponseDto<CustomerInfo>()
            {
                ErrorMessage = ErrorMessage.FailureToGetUserDeliveryInfo,
                ErrorCode = (int)ErrorCode.FailureToGetUserDeliveryInfo
            };
        }
    }

    public async Task<ResponseDto> ForgotPassword()
    {
        var userName = _httpContext.HttpContext.User.Identity.Name;
        var userFromCookie = new AppUser();
        if (userName != null)
        {
            userFromCookie = await _userManager.FindByNameAsync(userName);
        }
        var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == userFromCookie.Email);

        if (user == null)
        {
            return new ResponseDto()
            {
                ErrorMessage = ErrorMessage.UserDoesNotExist,
                ErrorCode = (int)ErrorCode.EmailNotFound
            };
        }

        try
        {
            string tokenForPasswordReseting = CreateRandomToken();
            await SendMessageWithToken(user,$"Message from ElectroStore\nPlease, enter this token to reset your password. Token will expire through 1 hour" +
                                            $"\n{tokenForPasswordReseting}");
            user.ResetPasswordToken = tokenForPasswordReseting;
            user.ResetPasswordTokenExpires = DateTime.Now.AddHours(1);

            await _context.SaveChangesAsync();

            return new ResponseDto()
            {
                SuccessMessage = SuccessMessage.TokenIsActivated
            };
        }
        catch (Exception e)
        {
            _logger.Error(e,e.Message);

            return new ResponseDto()
            {
                ErrorMessage = ErrorMessage.InternalServerError,
                ErrorCode = (int)ErrorCode.InternalServerError
            };
        }
    }

    public async Task<ResponseDto> ResetPassword(ResetPasswordDto resetPasswordDto)
    {
        var user =
            await _context.Users.FirstOrDefaultAsync(x => x.ResetPasswordToken == resetPasswordDto.TokenForPasswordReseting);

        if (user == null || user.ResetPasswordTokenExpires < DateTime.Now)
        {
            return new ResponseDto()
            {
                ErrorMessage = ErrorMessage.TokenIsNullOrNotFound,
                ErrorCode = (int)ErrorCode.TokenIsNullOrNotFound
            };
        }
        
        try
        {
            PasswordHasher<AppUser> passwordHasher = new PasswordHasher<AppUser>();
            user.PasswordHash = passwordHasher.HashPassword(user, resetPasswordDto.Password);;
            user.ResetPasswordToken = null;
            user.ResetPasswordTokenExpires = null;
            await _context.SaveChangesAsync();

            return new ResponseDto()
            {
                SuccessMessage = SuccessMessage.TokenIsActivated
            };
        }
        catch (Exception e)
        {
            _logger.Error(e,e.Message);
            return new ResponseDto()
            {
                ErrorMessage = ErrorMessage.InternalServerError,
                ErrorCode = (int)ErrorCode.InternalServerError
            };
        }
        
    }

    private async Task SendMessageWithToken(AppUser user,string message)
    {
        try
        {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_configuration.GetSection("EmailUsername").Value));
            email.To.Add(MailboxAddress.Parse(user.Email));
            email.Subject = "Message from ElectroStore";
            email.Body = new TextPart(TextFormat.Html) { Text = message };

            using var smtp = new SmtpClient();
        
            await smtp.ConnectAsync(_configuration.GetSection("EmailHost").Value, 587,SecureSocketOptions.StartTls);
        
            await smtp.AuthenticateAsync(_configuration.GetSection("EmailUsername").Value, _configuration.GetSection("EmailPassword").Value);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }
        catch (Exception e)
        {
            _logger.Error(e,e.Message);
        }
    }
    
    private string CreateRandomToken()
    {
        return Convert.ToHexString(RandomNumberGenerator.GetBytes((64)));
    }
}