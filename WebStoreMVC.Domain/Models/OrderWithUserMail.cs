using WebStoreMVC.Domain.Entities;

namespace WebStoreMVC.Models;

public class OrderWithUserMail
{
    public Order Order { get; set; }

    public string UserEmail { get; set; } = string.Empty;
}