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
    public string ProductName { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,2)")] 
    public decimal Price { get; set; }

    public int Quantity { get; set; }
    [MaxLength(1024)]
    [DataType(DataType.Text)]
    public string Description { get; set; } = string.Empty;
    [MaxLength(64)]
    public string Manufacturer { get; set; } = string.Empty;
    [MaxLength(64)]
    public string Colour { get; set; } = string.Empty;
    [MaxLength(128)]
    public string Hashtags { get; set; } = string.Empty;
    [MaxLength(128)]
    public string Images { get; set; } = string.Empty;
    
    public int CategoryId { get; set; }

    public string CategoryName { get; set; } = string.Empty;
}