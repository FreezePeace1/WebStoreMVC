using Microsoft.AspNetCore.Mvc;
using WebStoreMVC.Dtos;
using WebStoreMVC.Models.ViewModels;

namespace WebStoreMVC.Services.Interfaces;

public interface ICartService
{
    public ResponseDto<CartViewModel> Index();
    public Task<ResponseDto> Add(int id);
    public Task<ResponseDto> Decrease(int id);
    public Task<ResponseDto> Remove(int id);
    public ResponseDto Clear();

}