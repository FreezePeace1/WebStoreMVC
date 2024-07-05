using System.ComponentModel.DataAnnotations;

namespace WebStoreMVC.Dtos;

public class ResetPasswordDto
{
    [Required]
    public string TokenForPasswordReseting { get; set; } = string.Empty;

    [Required,MinLength(8)]
    public string Password { get; set; } = string.Empty;
    
    [Required,Compare(nameof(Password))]
    public string ConfirmPassword { get; set; } = string.Empty;
}