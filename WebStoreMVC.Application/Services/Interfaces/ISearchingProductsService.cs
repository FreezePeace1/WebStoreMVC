using WebStoreMVC.Domain.Entities;
using WebStoreMVC.Dtos;

namespace WebStoreMVC.Services.Interfaces;

public interface ISearchingProductsService
{
    public Task<ResponseDto<List<Product>>> SearchingProducts(string searchString = "");
}