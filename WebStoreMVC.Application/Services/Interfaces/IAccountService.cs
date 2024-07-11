using WebStoreMVC.Domain.Entities;
using WebStoreMVC.Dtos;
using WebStoreMVC.Models;

namespace WebStoreMVC.Services.Interfaces;

public interface IAccountService
{
    public Task<ResponseDto<List<ProductOrderModel>>> Index();

    public Task<ResponseDto<CustomerInfoDto>> ChangeInfo(CustomerInfoDto dto);
    public Task<ResponseDto<CustomerInfo>> ShowInfo();

    public Task<ResponseDto> ResetPassword(ResetPasswordDto dto);
    public Task<ResponseDto> ForgotPassword();
}