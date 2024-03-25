using System.ComponentModel.DataAnnotations;

namespace WebStoreMVC.Dtos;

public class UpdateDto
{
    [Required(ErrorMessage = "Нужно ввести логин")]
    public string Username { get; set; } = string.Empty;
}