using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebStoreMVC.Domain.Entities;

public class Product
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ProductId { get; set; }
    
    public int Article { get; set; }

    [MaxLength(512)] 
    [Column(TypeName = "text")] 
    public string ProductName { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,2)")] 
    public decimal Price { get; set; }

    public int Quantity { get; set; }
    [DataType(DataType.Text)]
    [Column(TypeName = "text")] 
    public string? Description { get; set; } = string.Empty;
    
    [MaxLength(128)]
    public string? Images { get; set; } = string.Empty;
    
    public int CategoryId { get; set; }
    public Category? Category { get; set; } 
    
    public int ColorId { get; set; }
    public Color? Color { get; set; }
    
    public int ManufacturerId { get; set; }
    public Manufacturer? Manufacturer { get; set; }
}