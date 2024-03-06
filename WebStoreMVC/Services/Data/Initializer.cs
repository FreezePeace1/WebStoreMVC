using Microsoft.AspNetCore.Identity;
using WebStoreMVC.Domain.Entities;

namespace WebStoreMVC.Services.Data;

public class Initializer
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public Initializer(UserManager<IdentityUser> userManager,RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
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

        if (await _userManager.FindByNameAsync(User.ADMINNAME) is null)
        {
            var admin = new IdentityUser()
            {
                UserName = User.ADMINNAME,
                Email = User.EMAIL
            };

            var createResult = await _userManager.CreateAsync(admin, User.PASSWORD);

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