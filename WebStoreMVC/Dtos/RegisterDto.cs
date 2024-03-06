using System.ComponentModel.DataAnnotations;
using NuGet.Protocol.Plugins;

namespace WebStoreMVC.Dtos;

public class RegisterDto
{
    [Required(ErrorMessage = "Нужно ввести логин")]
    public string Username { get; set; }
    
    [Required(ErrorMessage = "Нужно ввести пароль")]
    public string Password { get; set; }
    
    [Required(ErrorMessage = "Нужно ввести почту")]
    public string Email { get; set; }
}