using System.ComponentModel.DataAnnotations;

namespace WebStoreMVC.Dtos;

public class CustomerInfoDto
{
    [Required(ErrorMessage = "Требуется ввести номер телефона")/*,MinLength(11,ErrorMessage = "Мало символов")*/]
    [Phone(ErrorMessage = "Проверьте правильность ввода номера")]
    public string PhoneNumber { get; set; }
    [Required(ErrorMessage = "Требуется ввести фамилию")]
    public string LastName { get; set; } = string.Empty;
    [Required(ErrorMessage = "Требуется ввести имя")]
    public string FirstName { get; set; } = string.Empty;
    public string? Patronymic { get; set; } = string.Empty;
    [Required(ErrorMessage = "Требуется ввести город для доставки")]
    public string City { get; set; } = string.Empty;
    [Required(ErrorMessage = "Требуется ввести адрес для доставки")]
    public string Address { get; set; } = string.Empty;
    [Required(ErrorMessage = "Требуется ввести почту для доставки")]
    public string UserEmail { get; set; } = string.Empty;
    
    
}