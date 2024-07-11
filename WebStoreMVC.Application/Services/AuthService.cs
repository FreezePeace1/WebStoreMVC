using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MimeKit;
using MimeKit.Text;
using Serilog;
using WebStoreMVC.Application.Resources;
using WebStoreMVC.DAL.Context;
using WebStoreMVC.Domain.Entities;
using WebStoreMVC.Domain.Enum;
using WebStoreMVC.Dtos;
using WebStoreMVC.Models;
using WebStoreMVC.Services.Interfaces;

namespace WebStoreMVC.Application.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly IConfiguration _configuration;
    private readonly WebStoreContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger _logger;

    public AuthService(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager,
        SignInManager<AppUser> signInManager, IConfiguration configuration, WebStoreContext context,
        IHttpContextAccessor httpContextAccessor, ILogger logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _signInManager = signInManager;
        _configuration = configuration;
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task<ResponseDto> SeedRoles()
    {
        var isUserRoleExists = await _roleManager.RoleExistsAsync(UserRoles.USER);
        var isAdminRoleExists = await _roleManager.RoleExistsAsync(UserRoles.ADMINISTRATOR);

        if (isUserRoleExists && isAdminRoleExists)
        {
            return new ResponseDto()
            {
                ErrorMessage = ErrorMessage.RolesAlreadyExists,
                ErrorCode = (int)ErrorCode.RolesAlreadyExists
            };
        }

        try
        {
            await _roleManager.CreateAsync(new IdentityRole(UserRoles.USER));
            await _roleManager.CreateAsync(new IdentityRole(UserRoles.ADMINISTRATOR));

            return new ResponseDto()
            {
                SuccessMessage = SuccessMessage.CreatingRolesIsDone,
            };
        }
        catch (Exception e)
        {
            _logger.Error(e, e.Message);

            return new ResponseDto()
            {
                ErrorMessage = ErrorMessage.InternalServerError,
                ErrorCode = (int)ErrorCode.InternalServerError
            };
        }
    }

    public async Task<ResponseDto> Register(RegisterDto registerDto)
    {
        //Проверяем существует ли пользователь 
        if (await _userManager.FindByNameAsync(registerDto.Username) is not null)
        {
            return new ResponseDto()
            {
                ErrorMessage = $"{ErrorMessage.UserAlreadyExists} ({registerDto.Username})",
                ErrorCode = (int)ErrorCode.UserAlreadyExists
            };
        }

        /*await using var transaction = await _context.Database.BeginTransactionAsync();*/
        try
        {
            //Если пользователь существует,то создаем его
            var user = new AppUser
            {
                UserName = registerDto.Username,
                Email = registerDto.Email
            };

            var registerResult = await _userManager.CreateAsync(user, registerDto.Password);

            //Если создание прошло неудачно
            if (!registerResult.Succeeded)
            {
                var errors = registerResult.Errors.Select(e => e.Description);

                return new ResponseDto()
                {
                    ErrorMessage = $"{ErrorMessage.ModelCreatingIsFalied} \n {errors}",
                    ErrorCode = (int)ErrorCode.CreatingModelIsFailed
                };
            }

            //Добавляем роль пользователю
            await _userManager.AddToRoleAsync(user, UserRoles.USER);

            //For seeing roles after registration
            await _signInManager.SignInAsync(user, false);

            //Генерируем refresh token
            var refreshToken = GenerateRefreshToken();
            SetRefreshToken(refreshToken);

            user.RefreshToken = refreshToken.Token;
            user.TokenCreated = refreshToken.Created;
            user.TokenExpires = refreshToken.Expired;
            
            var accessToken = await GetAllAndSetAccessToken(user);
            
            //Запоминаем пользователя после регистрации
            await _signInManager.SignInAsync(user, true);

            //Отправляем сообщение пользователю что он зарегестрирован и токен для активации
            string tokenForVerification = CreateRandomToken();
            await SendMessageWithToken(user,$"You successfully registered!" +
                                            $"\nPlease activate token to verify your account:" +
                                            $"\n{tokenForVerification}");
            user.VerificationToken = tokenForVerification;
            
            await _context.SaveChangesAsync();

            /*await transaction.CommitAsync();*/
            
            return new ResponseDto()
            {
                SuccessMessage = $"{SuccessMessage.CreatingUserIsDone} ({user})",
            };
            
        }
        catch (Exception e)
        {
            _logger.Error(e,e.Message);

            return new ResponseDto()
            {
                ErrorMessage = ErrorMessage.AccessError,
                ErrorCode = (int)ErrorCode.AccessError
            };
        }
    }

    public async Task<ResponseDto> Login(LoginDto loginDto)
    {
        var user = await _userManager.FindByNameAsync(loginDto.Username);

        //Проверяем пароль и логин пользователя
        if (user is null)
        {
            return new ResponseDto()
            {
                ErrorMessage = ErrorMessage.IncorrectCredentials,
                ErrorCode = (int)ErrorCode.IncorrectCredentials
            };
        }

        if (!(await _userManager.CheckPasswordAsync(user, loginDto.Password)))
        {
            return new ResponseDto()
            {
                ErrorMessage = ErrorMessage.IncorrectCredentials,
                ErrorCode = (int)ErrorCode.IncorrectCredentials
            };
        }
        
        if (user.VerificationToken == null)
        {
            return new ResponseDto()
            {
                ErrorMessage = ErrorMessage.TokenIsNullOrNotFound
            };
        }

        /*await using var transaction = await _context.Database.BeginTransactionAsync();*/
        try
        {
            //Adding for getting access to user roles
            await _signInManager.PasswordSignInAsync(
                loginDto.Username,
                loginDto.Password,
                //После логина сохраняем данные Identity пользователя чтобы после закрытия браузера не проподали cookie
                true,
                false
            ); // если true то блокируем после всех попыток войти на аккаунт

            var accessToken = await GetAllAndSetAccessToken(user);
            
            RefreshToken existingRefreshToken = new RefreshToken()
            {
                Token = " ",
                Expired = DateTime.Now,
                Created = DateTime.Now
            };
            
            //Если refreshtoken оказался infinity в бд или его попросту нет
            if (user.RefreshToken.Equals("") || user.TokenExpires <= DateTime.MinValue ||
                user.TokenCreated <= DateTime.MinValue)
            {
                existingRefreshToken = GenerateRefreshToken();
                user.RefreshToken = existingRefreshToken.Token;
                user.TokenCreated = existingRefreshToken.Created;
                user.TokenExpires = existingRefreshToken.Expired;
                await _context.SaveChangesAsync();
            }
            //Если всё нормально с refreshtoken'ом в бд то просто передаем данные с бд
            else
            {
                existingRefreshToken.Token = user.RefreshToken;
                existingRefreshToken.Expired = user.TokenExpires;
                existingRefreshToken.Created = user.TokenCreated;
            }

            SetRefreshToken(existingRefreshToken);

            //Проверяем на срок истечения,если истекает то обновляем и записываем новый в БД
            if (user.TokenExpires < DateTime.Now || user.TokenExpires >= DateTime.MaxValue)
            {
                var refreshTokenFromCookies = _httpContextAccessor.HttpContext.Request.Cookies[CookieName.refreshToken];

                if (!user.RefreshToken.Equals(refreshTokenFromCookies) || user.TokenExpires < DateTime.Now)
                {
                    return new ResponseDto()
                    {
                        ErrorMessage = ErrorMessage.IncorrectToken,
                        ErrorCode = (int)ErrorCode.IncorrectToken
                    };
                }

                var newRefreshToken = GenerateRefreshToken();

                SetRefreshToken(newRefreshToken);
                user.RefreshToken = newRefreshToken.Token;
                await _context.SaveChangesAsync();
            }

            /*await transaction.CommitAsync();*/

            //Пользователь не активировал токен варификации, снова отправляем сообщение
            if (user.VerifiedAt == null)
            {
                await SendMessageWithToken(user,$"You successfully registered!" +
                                                $"\nPlease activate token to verify your account:" +
                                                $"\n{user.VerificationToken}");
                
                return new ResponseDto()
                {
                    SuccessMessage = ""
                };
            }

            return new ResponseDto()
            {
                SuccessMessage = $"{accessToken}",
            };
        }
        catch (Exception e)
        {
            _logger.Error(e,e.Message);

            return new ResponseDto()
            {
                ErrorMessage = ErrorMessage.InternalServerError,
                ErrorCode = (int)ErrorCode.InternalServerError
            };
        }
    }

    public async Task<string> SetAccessTokenForBackgroundService(AppUser user)
    {
        return await GetAllAndSetAccessToken(user);
    }

    public async Task<ResponseDto> VerifyAccount(string verificationToken)
    {
        var user = await _context.Users.FirstOrDefaultAsync(x => x.VerificationToken == verificationToken);

        if (user == null)
        {
            return new ResponseDto()
            {
                ErrorMessage = ErrorMessage.TokenIsNullOrNotFound,
                ErrorCode = (int)ErrorCode.TokenIsNullOrNotFound
            };
        }

        try
        {
            user.VerifiedAt = DateTime.Now;
            user.EmailConfirmed = true;
            await _context.SaveChangesAsync();

            return new ResponseDto()
            {
                SuccessMessage = SuccessMessage.TokenIsActivated
            };
        }
        catch (Exception e)
        {
            _logger.Error(e,e.Message);
            return new ResponseDto()
            {
                ErrorMessage = ErrorMessage.InternalServerError,
                ErrorCode = (int)ErrorCode.InternalServerError
            };
        }
    }

    public async Task<ResponseDto> ForgotPassword(string email)
    {
        var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == email);

        if (user == null)
        {
            return new ResponseDto()
            {
                ErrorMessage = ErrorMessage.UserDoesNotExist,
                ErrorCode = (int)ErrorCode.EmailNotFound
            };
        }

        try
        {
            string tokenForPasswordReseting = CreateRandomToken();
            await SendMessageWithToken(user,$"Message from ElectroStore\nPlease, enter this token to reset your password. Token will expire through 1 hour" +
                                            $"\n{tokenForPasswordReseting}");
            user.ResetPasswordToken = tokenForPasswordReseting;
            user.ResetPasswordTokenExpires = DateTime.Now.AddHours(1);

            await _context.SaveChangesAsync();

            return new ResponseDto()
            {
                SuccessMessage = SuccessMessage.TokenIsActivated
            };
        }
        catch (Exception e)
        {
            _logger.Error(e,e.Message);

            return new ResponseDto()
            {
                ErrorMessage = ErrorMessage.InternalServerError,
                ErrorCode = (int)ErrorCode.InternalServerError
            };
        }
    }

    public async Task<ResponseDto> ResetPassword(ResetPasswordDto resetPasswordDto)
    {
        var user =
            await _context.Users.FirstOrDefaultAsync(x => x.ResetPasswordToken == resetPasswordDto.TokenForPasswordReseting);

        if (user == null || user.ResetPasswordTokenExpires < DateTime.Now)
        {
            return new ResponseDto()
            {
                ErrorMessage = ErrorMessage.TokenIsNullOrNotFound,
                ErrorCode = (int)ErrorCode.TokenIsNullOrNotFound
            };
        }
        
        try
        {
            PasswordHasher<AppUser> passwordHasher = new PasswordHasher<AppUser>();
            user.PasswordHash = passwordHasher.HashPassword(user, resetPasswordDto.Password);;
            user.ResetPasswordToken = null;
            user.ResetPasswordTokenExpires = null;
            await _context.SaveChangesAsync();

            return new ResponseDto()
            {
                SuccessMessage = SuccessMessage.TokenIsActivated
            };
        }
        catch (Exception e)
        {
            _logger.Error(e,e.Message);
            return new ResponseDto()
            {
                ErrorMessage = ErrorMessage.InternalServerError,
                ErrorCode = (int)ErrorCode.InternalServerError
            };
        }
        
    }

    private async Task SendMessageWithToken(AppUser user,string message)
    {
        try
        {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_configuration.GetSection("EmailUsername").Value));
            email.To.Add(MailboxAddress.Parse(user.Email));
            email.Subject = "Message from ElectroStore";
            email.Body = new TextPart(TextFormat.Html) { Text = message };

            using var smtp = new SmtpClient();
        
            await smtp.ConnectAsync(_configuration.GetSection("EmailHost").Value, 587,SecureSocketOptions.StartTls);
            /*smtp.AuthenticationMechanisms.Remove("XOAUTH2");*/
        
            await smtp.AuthenticateAsync(_configuration.GetSection("EmailUsername").Value, _configuration.GetSection("EmailPassword").Value);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }
        catch (Exception e)
        {
            _logger.Error(e,e.Message);
        }
    }
    
    private string CreateRandomToken()
    {
        return Convert.ToHexString(RandomNumberGenerator.GetBytes((64)));
    }

    private async Task<string> GetAllAndSetAccessToken(AppUser user)
    {
        var userRoles = await _userManager.GetRolesAsync(user);

        //Создаем доп инфу для пользователя во время авторизации
        var claims = new List<Claim>()
        {
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim("JWTID", Guid.NewGuid().ToString())
        };

        //Передаем всем пользователям клеймы
        foreach (var userRole in userRoles)
        {
            claims.Add(new Claim(ClaimTypes.Role, userRole));
        }

        var accessToken = GenerateJsonWebToken(claims);
        SetAccessToken(accessToken);

        return accessToken;
    }
    
    private string GenerateJsonWebToken(List<Claim> claims)
    {
        var secret = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Key"]));
        var tokenObject = new JwtSecurityToken(
            issuer: _configuration["JwtSettings:Issuer"],
            audience: _configuration["JwtSettings:Audience"],
            expires: DateTime.Now.AddMinutes(20),
            claims: claims,
            signingCredentials: new SigningCredentials(secret, SecurityAlgorithms.HmacSha256));
        
        string token = new JwtSecurityTokenHandler().WriteToken(tokenObject);
        
        return token;
    }

    private RefreshToken GenerateRefreshToken()
    {
        var refreshToken = new RefreshToken
        {
            Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
            Expired = DateTime.Now.AddDays(30),
            Created = DateTime.Now
        };

        return refreshToken;
    }

    private void SetRefreshToken(RefreshToken refreshToken)
    {
        var cookieOpt = new CookieOptions
        {
            HttpOnly = true,
            //Для передачи только по https
            /*Secure = true,*/
            Expires = refreshToken.Expired
        };
        
        _httpContextAccessor.HttpContext.Response.Cookies.Append(CookieName.refreshToken, refreshToken.Token, cookieOpt);
    }

    private void SetAccessToken(string accessToken)
    {
        var cookieOpt = new CookieOptions
        {
            HttpOnly = true,
            //Для передачи только по https
            /*Secure = true,*/
            Expires = DateTime.Now.AddMinutes(20)
        };

        _httpContextAccessor.HttpContext.Response.Cookies.Append(CookieName.accessToken, accessToken, cookieOpt);
        _httpContextAccessor.HttpContext.Response.Cookies.Append(CookieName.accessTokenExpires,DateTime.Now.AddMinutes(20).Date.ToString(CultureInfo.CurrentCulture));
        
    }

    public async Task<ResponseDto> FromUserToAdmin(UpdateDto updateDto)
    {
        //Проверяем есть ли пользователь
        var user = await _userManager.FindByNameAsync(updateDto.Username);

        if (user is null)
        {
            return new ResponseDto()
            {
                ErrorMessage = $"{ErrorMessage.UserDoesNotExist} ({updateDto.Username})",
                ErrorCode = (int)ErrorCode.UserDoesNotExist
            };
        }

        if (updateDto.Username == AdminUser.ADMINNAME)
        {
            return new ResponseDto
            {
                ErrorMessage = $"{ErrorMessage.UserAlreadyIsAdmin} ({updateDto.Username})",
                ErrorCode = (int)ErrorCode.UserAlreadyIsAdmin
            };
        }

        try
        {
            await _userManager.AddToRoleAsync(user, UserRoles.ADMINISTRATOR);

            return new ResponseDto()
            {
                SuccessMessage = $"{SuccessMessage.UpgradeUserToAdmin} ({user})",
            };
        }
        catch (Exception e)
        {
            _logger.Error(e,e.Message);

            return new ResponseDto()
            {
                ErrorMessage = ErrorMessage.InternalServerError,
                ErrorCode = (int)ErrorCode.InternalServerError
            };
        }
    }

    public async Task<ResponseDto> FromAdminToUser(UpdateDto updateDto)
    {
        var user = await _userManager.FindByNameAsync(updateDto.Username);

        if (user is null)
        {
            return new ResponseDto
            {
                ErrorMessage = $"{ErrorMessage.UserDoesNotExist} ({updateDto.Username})",
                ErrorCode = (int)ErrorCode.UserDoesNotExist
            };
        }

        if (updateDto.Username == AdminUser.ADMINNAME)
        {
            return new ResponseDto
            {
                ErrorMessage = ErrorMessage.AccessError,
                ErrorCode = (int)ErrorCode.AccessError
            };
        }

        try
        {
            await _userManager.RemoveFromRoleAsync(user, UserRoles.ADMINISTRATOR);

            return new ResponseDto
            {
                SuccessMessage = $"{SuccessMessage.DowngradeAdminToUser} ({updateDto.Username})"
            };
        }
        catch (Exception e)
        {
            _logger.Error(e,e.Message);

            return new ResponseDto()
            {
                ErrorMessage = ErrorMessage.InternalServerError,
                ErrorCode = (int)ErrorCode.InternalServerError
            };
        }
    }

    public async Task<ResponseDto> Logout()
    {
        await _signInManager.SignOutAsync();
        _httpContextAccessor.HttpContext.Response.Cookies.Delete(CookieName.accessToken);
        _httpContextAccessor.HttpContext.Response.Cookies.Delete(CookieName.refreshToken);
        _httpContextAccessor.HttpContext.Response.Cookies.Delete(CookieName.accessTokenExpires);

        return new ResponseDto()
        {
            SuccessMessage = SuccessMessage.LogoutIsDone
        };
    }
}