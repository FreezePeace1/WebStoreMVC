using System.ComponentModel.DataAnnotations;

namespace WebStoreMVC.Dtos;

public class ForgotPasswordDto
{
    [Required,EmailAddress]
    public string Email { get; set; } = string.Empty;
}