using System.ComponentModel.DataAnnotations.Schema;

namespace WebStoreMVC.Domain.Entities;

public class Product
{
    public int Id { get; set; }
    public int Article { get; set; }
    public string ProductName { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    public int Quantity { get; set; }
    public string Description { get; set; }
    public string Manufacturer { get; set; }
    public string Colour { get; set; }
    public string Hashtags { get; set; }
    public string Images { get; set; }
}