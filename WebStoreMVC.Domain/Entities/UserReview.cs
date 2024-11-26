using System.ComponentModel.DataAnnotations;

namespace WebStoreMVC.Domain.Entities;

public class UserReview
{
    public int Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public string ReviewDescription { get; set; } = string.Empty;
    public int Rating { get; set; }
    public DateTime ReviewDateTime { get; set; }
    
    [DataType("Text")]
    public string AppUserId { get; set; }

    public AppUser AppUser { get; set; } = null!;
    
    public int ProductId { get; set; } 
}