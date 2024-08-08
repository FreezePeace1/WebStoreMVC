using WebStoreMVC.Domain.Entities;

namespace WebStoreMVC.Models;

public class ProductSearchingModel
{
    public IQueryable<Product> Products { get; set; }
    
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public int StartedPage { get; set; }
    public int EndedPage { get; set; }
    public string SearchString { get; set; } = string.Empty;
}