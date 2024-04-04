using WebStoreMVC.Domain.Interfaces;
using WebStoreMVC.Dtos;
using WebStoreMVC.Models;
using WebStoreMVC.Services.Interfaces;

namespace WebStoreMVC.Services;

public class CartService : ICartService
{
    private readonly ICartStore _cartStore;
    private readonly IProductsService _productsService;

    public CartService(ICartStore cartStore,IProductsService productsService)
    {
        this._cartStore = cartStore;
        _productsService = productsService;
    }
    
    public void AddToCart(int id)
    {
        var cart = _cartStore.Cart;
        var item = cart.Items.FirstOrDefault(p => p.Id == id);

        if (item == null)
        {
            cart.Items.Add(new ItemCart() {Id = id,Quantity = 1});
        }
        else
        {
            item.Quantity++;
        }

        _cartStore.Cart = cart;
    }

    public void DecrementFromCart(int id)
    {
        var cart = _cartStore.Cart;
        var item = cart.Items.FirstOrDefault(p => p.Id == id);

        if (item == null)
        {
            return;
        }

        if (item.Quantity > 0)
        {
            item.Quantity--;
        }

        if (item.Quantity == 0)
        {
            cart.Items.Remove(item);
        }

        _cartStore.Cart = cart;


    }

    public void RemoveFromCart(int id)
    {
        var cart = _cartStore.Cart;
        var item = cart.Items.FirstOrDefault(p => p.Id == id);

        if (item == null)
        {
            return;
        }

        cart.Items.Remove(item); 
        _cartStore.Cart = cart;
    }

    public void RemoveAll()
    {
        _cartStore.Cart.Items.Clear();
    }

    public CartDto TransformFromCart()
    {
        var cartItems = _cartStore.Cart;
        var products = _productsService.GetAllProducts();

        return new CartDto()
        {
        };
        //TODO
    }
}