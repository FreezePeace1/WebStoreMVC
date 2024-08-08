using Stripe.Checkout;
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

    public Task<ResponseDto<Session>> StripePayment();
    public Task<ResponseDto<Order>> GetLastOrder();
    public Task<ResponseDto<List<ProductOrderModel>>> FindUserOrder(string id);
    public Task<ResponseDto> SendOrderToUserEmail(OrderWithUserMail orderWithUserMail);
    public Task<ResponseDto> SendUserInfoAfterSuccessfulOrder(RegisterDto dto);

    /*public Task<ResponseDto<UpdateUserOrderDto>> UpdateUserOrder(int id);*/
}