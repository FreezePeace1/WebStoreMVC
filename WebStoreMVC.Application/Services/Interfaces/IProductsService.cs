using WebStoreMVC.Domain.Entities;
using WebStoreMVC.Dtos;

namespace WebStoreMVC.Services.Interfaces;

public interface IProductsService
{
    public Task<List<Product>> GetAllProducts();

    public Task<ResponseDto<Product>> GetProductById(int id);

    public Task<ResponseDto<Product>> PostProduct(Product product);

    public Product Edit(int id);

    public Task<ResponseDto> EditProduct(Product product);

    public Task<ResponseDto> DeleteProduct(int? id);

    public Task<ResponseDto<List<Product>>> GetProductByPage(int page, int pageSize);

}