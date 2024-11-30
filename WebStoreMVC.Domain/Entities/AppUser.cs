using Microsoft.AspNetCore.Identity;

namespace WebStoreMVC.Domain.Entities;

public class AppUser : IdentityUser
{
    public string RefreshToken { get; set; } = string.Empty;
    
    public DateTime TokenCreated { get; set; }
    
    public DateTime TokenExpires { get; set; }

    public string? ResetPasswordToken { get; set; }
    
    public DateTime? ResetPasswordTokenExpires { get; set; }
    
    public string? VerificationToken { get; set; }
    
    public DateTime? VerifiedAt { get; set; }
    
    public List<UserReview>? UserReview { get; set; }
    
    public List<CustomerInfo>? CustomerInfo { get; set; }
    
    public List<Order>? Order { get; set; }
}