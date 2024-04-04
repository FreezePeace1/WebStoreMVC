using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using WebStoreMVC.Domain.Interfaces;
using WebStoreMVC.Models;

namespace WebStoreMVC.Services;

public class CookiesCart : ICartStore
{
    private readonly HttpContextAccessor _httpContextAccessor;
    private readonly string _cartName;

    public CookiesCart(HttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;

        _cartName = "cart" + (_httpContextAccessor.HttpContext.User.Identity.IsAuthenticated
            ? _httpContextAccessor.HttpContext.User.Identity.Name
            : "");
    }

    public CartModel Cart
    {
        get
        {
            var cookie = _httpContextAccessor.HttpContext.Request.Cookies[_cartName];
            string json = string.Empty;
            CartModel cart = null;

            if (cookie == null)
            {
                cart = new CartModel()
                {
                    Items = new List<ItemCart>()
                };

                json = JsonConvert.SerializeObject(cart);

                _httpContextAccessor.HttpContext.Response.Cookies.Append(_cartName,json, new CookieOptions()
                {
                    Expires = DateTime.Now.AddDays(30)
                });
            }

            json = cookie;
            cart = JsonConvert.DeserializeObject<CartModel>(json);
            
            _httpContextAccessor.HttpContext.Response.Cookies.Delete(_cartName);
            
            _httpContextAccessor.HttpContext.Response.Cookies.Append(_cartName,json, new CookieOptions()
            {
                Expires = DateTime.Now.AddDays(30)
            });

            return cart;
        }

        set
        {
            var json = JsonConvert.SerializeObject(value);
            
            _httpContextAccessor.HttpContext.Response.Cookies.Delete(_cartName);
            
            _httpContextAccessor.HttpContext.Response.Cookies.Append(_cartName,json, new CookieOptions()
            {
                Expires = DateTimeOffset.Now.AddDays(30)
            });
        }
    }
}