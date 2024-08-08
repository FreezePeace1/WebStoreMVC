using System.ComponentModel.DataAnnotations;

namespace WebStoreMVC.Models;

public class FindOrderModel
{
    [Required, MinLength(32, ErrorMessage = "Проверьте правильность ввода")]
    public string OrderId { get; set; } = string.Empty;
}