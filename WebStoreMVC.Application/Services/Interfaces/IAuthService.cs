using WebStoreMVC.Dtos;

namespace WebStoreMVC.Services.Interfaces;

public interface IAuthService
{
    public Task<ResponseDto> SeedRoles();
    public Task<ResponseDto> Register(RegisterDto registerDto);

    public Task<ResponseDto> FromUserToAdmin(UpdateDto updateDto);
    public Task<ResponseDto> FromAdminToUser(UpdateDto updateDto);

    public Task<ResponseDto> Login(LoginDto loginDto);

    public Task<ResponseDto> Logout();
}