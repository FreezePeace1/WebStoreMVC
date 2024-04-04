namespace WebStoreMVC.Models;

public class CartModel
{
    public List<ItemCart> Items { get; set; }
    public int Count => Items?.Sum(x => x.Quantity) ?? 0;
}