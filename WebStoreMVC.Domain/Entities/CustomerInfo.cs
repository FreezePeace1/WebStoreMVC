using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebStoreMVC.Domain.Entities;

public class CustomerInfo
{
    public int Id { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string? Patronymic { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;

    public string UserEmail { get; set; } = string.Empty;
    
    [DataType("Text")]
    public string? AppUserId { get; set; }
    public AppUser AppUser { get; set; } = null!;
}