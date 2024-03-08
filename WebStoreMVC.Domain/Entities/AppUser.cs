using Microsoft.AspNetCore.Identity;

namespace WebStoreMVC.Domain.Entities;

public class AppUser : IdentityUser
{
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime TokenCreated { get; set; }
    public DateTime TokenExpires { get; set; }
}