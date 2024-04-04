using WebStoreMVC.Models;

namespace WebStoreMVC.Domain.Interfaces;

public interface ICartStore
{
    CartModel Cart { get; set; }
}