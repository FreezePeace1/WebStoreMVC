using System.ComponentModel.DataAnnotations;

namespace WebStoreMVC.Dtos;

public class PostReviewDto
{
    [Required(ErrorMessage = "Нужно написать имя")]
    public string UserName { get; set; } = string.Empty;
    [Required(ErrorMessage = "Нужно написать почту"),EmailAddress]
    public string UserEmail { get; set; } = string.Empty;

    [Required(ErrorMessage = "Нужно написать отзыв")]
    public string ReviewDescription { get; set; } = string.Empty;
    [Required(ErrorMessage = "Нужно поставить оценку")]
    public int Rating { get; set; }
}