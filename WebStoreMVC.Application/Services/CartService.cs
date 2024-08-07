using Microsoft.AspNetCore.Http;
using Serilog;
using WebStoreMVC.Application.JSON;
using WebStoreMVC.Application.Resources;
using WebStoreMVC.DAL.Context;
using WebStoreMVC.Domain.Enum;
using WebStoreMVC.Dtos;
using WebStoreMVC.Models;
using WebStoreMVC.Models.ViewModels;
using WebStoreMVC.Services.Interfaces;

namespace WebStoreMVC.Application.Services;

public class CartService : ICartService
{
    private readonly WebStoreContext _dbcontext;
    private readonly IHttpContextAccessor _httpContext;
    private readonly ILogger _logger;

    public CartService(WebStoreContext dbcontext,IHttpContextAccessor httpContext,ILogger logger)
    {
        _dbcontext = dbcontext;
        _httpContext = httpContext;
        _logger = logger;
    }
    
    public ResponseDto<CartViewModel> Index()
    {
        List<CartItem> cart = _httpContext.HttpContext.Session.GetJson<List<CartItem>>("Cart")
                              ?? new List<CartItem>();

        CartViewModel cartVM = new()
        {
            CartItems = cart,
            GrandTotal = cart.Sum(x => x.Price * x.Quantity)
        };

        return  new ResponseDto<CartViewModel>()
        {
            Data = cartVM,
            SuccessMessage = SuccessMessage.CreatingCartVM,
        };
    }

    public async Task<ResponseDto> Add(int id)
    {
        try
        {
            var product = await _dbcontext.Products.FindAsync(id);

            List<CartItem> cart = _httpContext.HttpContext.Session.GetJson<List<CartItem>>("Cart")
                                  ?? new List<CartItem>();

            CartItem cartItem = cart.Where(x => x.ProductId == id).FirstOrDefault();

            if (cartItem == null)
            {
                cart.Add(new CartItem(product));
            }
            else
            {
                cartItem.Quantity++;
            }
        
            _httpContext.HttpContext.Session.SetJson("Cart",cart);

            return new ResponseDto()
            {
                SuccessMessage = SuccessMessage.OperationForCart
            };

        }
        catch (Exception e)
        {
            _logger.Error(e,e.Message);
            return new ResponseDto()
            {
                ErrorMessage = ErrorMessage.FailureToAddProductToCart,
                ErrorCode = (int)ErrorCode.FailureToAddProductToCart
            };
        }
        
    }

    public async Task<ResponseDto> Decrease(int id)
    {
        List<CartItem> cart = _httpContext.HttpContext.Session.GetJson<List<CartItem>>("Cart")
                              ?? new List<CartItem>();

        CartItem cartItem = cart.Where(x => x.ProductId == id).FirstOrDefault();

        if (cartItem.Quantity > 1)
        {
            cartItem.Quantity--;
        }
        else
        {
            cart.RemoveAll(x => x.ProductId == id);
        }

        if (cart.Count == 0)
        {
            _httpContext.HttpContext.Session.Remove("Cart");
        }
        else
        {
            _httpContext.HttpContext.Session.SetJson("Cart", cart);
        }

        return new ResponseDto()
        {   
            SuccessMessage = SuccessMessage.OperationForCart
        };

    }

    public async Task<ResponseDto> Remove(int id)
    {
        List<CartItem> cart = _httpContext.HttpContext.Session.GetJson<List<CartItem>>("Cart");

        cart.RemoveAll(x => x.ProductId == id);

        if (cart.Count == 0)
        {
            _httpContext.HttpContext.Session.Remove("Cart");
        }
        else
        {
            _httpContext.HttpContext.Session.SetJson("Cart",cart);
        }

        return new ResponseDto()
        {
            SuccessMessage = SuccessMessage.OperationForCart
        };

    }

    public ResponseDto Clear()
    {
        _httpContext.HttpContext.Session.Remove("Cart");

        return new ResponseDto()
        {
            SuccessMessage = SuccessMessage.OperationForCart
        };
    }
}