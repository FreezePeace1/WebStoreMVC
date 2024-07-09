using WebStoreMVC.Domain.Entities;

namespace WebStoreMVC.Dtos;

public class UpdateUserOrderDto
{
    public List<Product> products { get; set; }
}