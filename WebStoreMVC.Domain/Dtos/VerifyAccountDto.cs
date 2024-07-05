using System.ComponentModel.DataAnnotations;

namespace WebStoreMVC.Dtos;

public class VerifyAccountDto
{
    [Required]
    public string VerificationToken { get; set; } = string.Empty;
}