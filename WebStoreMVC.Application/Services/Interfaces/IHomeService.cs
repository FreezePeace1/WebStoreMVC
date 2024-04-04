using WebStoreMVC.Domain.Entities;

namespace WebStoreMVC.Services.Interfaces;

public interface IHomeService
{
    public Task<List<Product>> Store();
}