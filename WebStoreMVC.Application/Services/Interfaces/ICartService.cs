using Microsoft.AspNetCore.Mvc;
using WebStoreMVC.Dtos;

namespace WebStoreMVC.Services.Interfaces;

public interface ICartService
{
    public ResponseDto Index();
    public Task<ResponseDto> Add(int id);
    public Task<ResponseDto> Decrease(int id);
    public Task<ResponseDto> Remove(int id);
    public ResponseDto Clear();

}