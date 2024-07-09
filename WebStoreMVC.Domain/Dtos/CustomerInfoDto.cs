using System.ComponentModel.DataAnnotations;

namespace WebStoreMVC.Dtos;

public class CustomerInfoDto
{
    [Required/*,MinLength(11,ErrorMessage = "Мало символов")*/]
    [Phone]
    public string PhoneNumber { get; set; }
    [Required]
    public string LastName { get; set; } = string.Empty;
    [Required]
    public string FirstName { get; set; } = string.Empty;

    public string? Patronymic { get; set; } = string.Empty;
    [Required]
    public string City { get; set; } = string.Empty;
    [Required]
    public string Address { get; set; } = string.Empty;
}