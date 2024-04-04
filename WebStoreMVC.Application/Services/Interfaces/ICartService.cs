using WebStoreMVC.Dtos;
using WebStoreMVC.Models;

namespace WebStoreMVC.Services.Interfaces;

public interface ICartService
{
    public void AddToCart(int id);
    public void DecrementFromCart(int id);
    public void RemoveFromCart(int id);
    public void RemoveAll();
    public CartDto TransformFromCart();
}