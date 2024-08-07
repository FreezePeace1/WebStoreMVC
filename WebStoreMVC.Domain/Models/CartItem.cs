using WebStoreMVC.Domain.Entities;

namespace WebStoreMVC.Models;

public class CartItem
{
    public int ProductId { get; set; }
    
    public string ProductName { get; set; }
    
    public decimal Price { get; set; }
    
    public int Quantity { get; set; }

    public decimal Total
    {
        get
        {
            return Quantity * Price;
        }
    }
    
    public string Image { get; set; }

    public CartItem()
    {
    }

    public CartItem(Product product)
    {
        ProductId = product.ProductId;
        ProductName = product.ProductName;
        Price = product.Price;
        Quantity = 1;
        Image = product.Images;
    }
}