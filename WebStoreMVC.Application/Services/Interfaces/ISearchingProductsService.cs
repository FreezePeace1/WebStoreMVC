using WebStoreMVC.Domain.Entities;
using WebStoreMVC.Dtos;
using WebStoreMVC.Models;

namespace WebStoreMVC.Services.Interfaces;

public interface ISearchingProductsService
{
    public Task<ResponseDto<ProductSearchingModel>> SearchingProducts(/*ProductFilter? productFilter,*/string searchString = "",int currentPage = 1);
}