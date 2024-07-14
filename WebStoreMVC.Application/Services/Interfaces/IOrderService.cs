using WebStoreMVC.Domain.Entities;
using WebStoreMVC.Dtos;
using WebStoreMVC.Models;

namespace WebStoreMVC.Services.Interfaces;

public interface IOrderService
{
    public Task<ResponseDto<CustomerInfo>> SaveCustomerInfo(CustomerInfoDto dto);

    public ResponseDto<List<CartItem>> ShowCartInfo();

    public Task<ResponseDto> DeleteUserOrder(string id);

    public Task<ResponseDto> SaveUserOrder();

    public Task<ResponseDto> StripePayment();

    /*public Task<ResponseDto<UpdateUserOrderDto>> UpdateUserOrder(int id);*/
}