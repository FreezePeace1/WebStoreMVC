using WebStoreMVC.Domain.Entities;

namespace WebStoreMVC.Dtos;

public class CartDto
{
    public Dictionary<Product, int> Items { get; set; } = new Dictionary<Product, int>();

    public int ItemsCount => Items?.Sum(x => x.Value) ?? 0;
}