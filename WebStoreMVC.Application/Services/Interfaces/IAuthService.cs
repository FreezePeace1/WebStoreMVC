using WebStoreMVC.Domain.Entities;
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

    public Task<string> SetAccessTokenForMiddleware(AppUser user);
    
    public Task<ResponseDto> VerifyAccount(string verificationToken);

    public Task<ResponseDto> ForgotPassword(string email);

    public Task<ResponseDto> ResetPassword(ResetPasswordDto resetPasswordDto);
    public Task<bool> IsUserExists(string userEmail);
}