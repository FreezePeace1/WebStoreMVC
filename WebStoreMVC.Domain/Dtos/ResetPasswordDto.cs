using System.ComponentModel.DataAnnotations;

namespace WebStoreMVC.Dtos;

public class ResetPasswordDto
{
    [Required(ErrorMessage = "Нужен токен для сброса пароля")]
    public string TokenForPasswordReseting { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введите пароль"),MinLength(8,ErrorMessage = "Минимальное кол-во символов должно быть не меньше 8")]
    public string Password { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Проверьте правильность пароля"),Compare(nameof(Password))]
    public string ConfirmPassword { get; set; } = string.Empty;
}