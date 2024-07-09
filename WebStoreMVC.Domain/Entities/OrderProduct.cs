using System.ComponentModel.DataAnnotations;

namespace WebStoreMVC.Domain.Entities;

public class OrderProduct
{
    [Key]
    public int Id { get; set; }
    
    public int ProductId { get; set; }
    public Product Product { get; set; }
    public int ProductCount { get; set; }
    public string OrderId { get; set; }
    public Order Order { get; set; }
    
}