using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using WebStoreMVC.Domain.Entities;

namespace WebStoreMVC.Services.Data;

public class Initializer
{
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IConfiguration _configuration;

    public Initializer(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager,IConfiguration configuration)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _configuration = configuration;
    }

    public void Initialize()
    {
        InitializeAsync().Wait();
    }

    public async Task InitializeAsync()
    {
        await GenerateIdentity().ConfigureAwait(false);
    }

    private async Task GenerateIdentity()
    {
        if (!await _roleManager.RoleExistsAsync(UserRoles.ADMINISTRATOR))
        {
            await _roleManager.CreateAsync(new IdentityRole()
            {
                Name = UserRoles.ADMINISTRATOR
            });
        }

        if (!await _roleManager.RoleExistsAsync(UserRoles.USER))
        {
            await _roleManager.CreateAsync(new IdentityRole()
            {
                Name = UserRoles.USER
            });
        }

        var adminName = _configuration["AdminUser:ADMINNAME"];
        var adminEmail = _configuration["AdminUser:EMAIL"];
        var adminPassword = _configuration.GetSection("AdminUser:PASSWORD").Value;
        
        if (await _userManager.FindByNameAsync(adminName) is null)
        {
            var admin = new AppUser()
            {
                UserName = adminName,
                Email = adminEmail,
                VerificationToken = "HasAutomaticallyForAdmin",
                EmailConfirmed = true,
                VerifiedAt = DateTime.Now
            };

            var createResult = await _userManager.CreateAsync(admin, adminPassword);

            if (createResult.Succeeded)
            {
                await _userManager.AddToRoleAsync(admin, UserRoles.ADMINISTRATOR);
            }
            else
            {
                var errors = createResult.Errors.Select(error => error.Description);
                throw new InvalidOperationException($"Ошибка при создании Админа" +
                                                    $"{string.Join(",", errors)}");
            }
        }
    }
}