using WebStoreMVC.Domain.Entities;
using WebStoreMVC.Dtos;
using WebStoreMVC.Models;

namespace WebStoreMVC.Services.Interfaces;

public interface IHomeService
{
    public Task<ResponseDto<List<Product>>> Store();

    public Task<ResponseDto<AllInfoProductModel>> ShowProductInfo(int id,int currentPage = 1);
}