using Microsoft.AspNetCore.Identity;
using WebStoreMVC;
using WebStoreMVC.DAL.Context;
using WebStoreMVC.DAL.DependencyInjection;
using WebStoreMVC.Domain.Entities;
using WebStoreMVC.Services;
using WebStoreMVC.Services.Data;
using WebStoreMVC.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

//Add DB connection
builder.Services.AddDataAccessLayer(builder.Configuration);

builder.Services.AddAuthenticationAndAuthorization(builder);

builder.Services.AddSwagger();

builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();

//Подключаем Identity
builder.Services.AddIdentity<AppUser, IdentityRole>()
    .AddEntityFrameworkStores<WebStoreContext>()
    .AddDefaultTokenProviders();

//Настраиваем Identity
builder.Services.Configure<IdentityOptions>(opt =>
{
    opt.Password.RequireDigit = true;
    opt.Password.RequiredLength = 8;
    opt.Password.RequireLowercase = true;
    opt.Password.RequireUppercase = false;
    opt.Password.RequireNonAlphanumeric = false;

    //Не реализовано
    opt.Lockout.AllowedForNewUsers = true;
    opt.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
    opt.Lockout.MaxFailedAccessAttempts = 5;

    opt.User.RequireUniqueEmail = false;
});


//Подключаем сервис аккаунта
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<Initializer>();


AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var app = builder.Build();

app.UseSwagger()
    .UseSwaggerUI();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();
app.UseAuthentication();

//Чтобы сервис включался при запуске (добавляем автоматически роли и админа если этого нет в БД)
var scope = app.Services.CreateScope();
var service = scope.ServiceProvider.GetService<Initializer>();
service.Initialize();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


app.Run();