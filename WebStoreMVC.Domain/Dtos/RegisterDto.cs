using System.ComponentModel.DataAnnotations;

namespace WebStoreMVC.Dtos;

public class RegisterDto
{
    [Required(ErrorMessage = "Нужно ввести логин")]
    public string Username { get; set; } = string.Empty;
     
    [Required(ErrorMessage = "Нужно ввести пароль")]
    public string Password { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Повторите пароль")]
    //Сравниваем с введенным паролем
    [Compare(nameof(Password))]
    public string ConfirmedPassword { get; set; } = string.Empty;
    
    public string Email { get; set; } = string.Empty;
}