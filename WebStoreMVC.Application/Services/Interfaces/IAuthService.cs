using WebStoreMVC.Dtos;

namespace WebStoreMVC.Services.Interfaces;

public interface IAuthService
{
   public Task<AuthResponseDto> SeedRoles();
   public Task<AuthResponseDto> Register(RegisterDto registerDto);
   /*public Task<AuthResponseDto> Login(LoginDto loginDto);*/
   public Task<AuthResponseDto> FromUserToAdmin(UpdateDto updateDto);
   public Task<AuthResponseDto> FromAdminToUser(UpdateDto updateDto);

   public Task<AuthResponseDto> Login(LoginDto loginDto);

}