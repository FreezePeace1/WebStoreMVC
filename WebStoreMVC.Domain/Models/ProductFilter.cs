namespace WebStoreMVC.Models;

public class ProductFilter
{
    public string? Color { get; set; }
    
    public string? Category { get; set; }
    
    public string? Manufacturer { get; set; }
    
    public decimal? MinPrice { get; set; }
    
    public decimal? MaxPrice { get; set; }
}