using System.ComponentModel.DataAnnotations;

namespace WebStoreMVC.Dtos;

public class LoginDto
{
    [Required(ErrorMessage = "Нужно ввести логин")]
    [MaxLength(64)]
    [Display(Name = "Имя пользователя")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Нужно ввести пароль")]
    [MaxLength(64)]
    [DataType(DataType.Password)]
    [Display(Name = "Пароль")]
    public string Password { get; set; } = string.Empty;

    /*//Помнить ли пользователя
    [Display(Name = "Запомнить меня")]
    public bool RememberMe { get; set; }

    //Возвращаем пользователя на тот путь куда он хотел попасть после регистрации
    [HiddenInput(DisplayValue = false)]
    public string ReturnUrl { get; set;  } = string.Empty;*/
}