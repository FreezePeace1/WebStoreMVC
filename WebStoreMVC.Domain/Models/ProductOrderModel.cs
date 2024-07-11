using WebStoreMVC.Domain.Entities;

namespace WebStoreMVC.Models;

public class ProductOrderModel
{
    public string OrderId { get; set; } = string.Empty;
    public decimal TotalPrice { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public string Colour { get; set; } = string.Empty;
    public string Images { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
}