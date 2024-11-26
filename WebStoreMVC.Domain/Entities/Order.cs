using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebStoreMVC.Domain.Entities;

public class Order
{
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    [Key]
    public string OrderId { get; set; }
    public int TotalPrice { get; set; }
    public DateTime OrderDate { get; set; }
    public string OrderStatus { get; set; } = string.Empty;
    
    [DataType("Text")]
    public string? AppUserId { get; set; }
    
    public AppUser AppUser { get; set; } = null!;
    
}