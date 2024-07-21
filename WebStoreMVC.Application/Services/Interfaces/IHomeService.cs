using WebStoreMVC.Domain.Entities;
using WebStoreMVC.Dtos;

namespace WebStoreMVC.Services.Interfaces;

public interface IHomeService
{
    public Task<ResponseDto<List<Product>>> Store();

    public Task<ResponseDto<Product>> GetProductById(int id);
}