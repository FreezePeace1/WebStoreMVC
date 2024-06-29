using Serilog;
using WebStoreMVC;
using WebStoreMVC.Application.DependencyInjection;
using WebStoreMVC.DAL.DependencyInjection;
using WebStoreMVC.Middleware;
using WebStoreMVC.Services.Data;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
//From DAL
//Add DB connection
builder.Services.AddDataAccessLayer(builder.Configuration);

//From Startup
builder.Services.AddAuthenticationAndAuthorization(builder);
builder.Services.AddSwagger();
builder.Services.AddIdentity();

builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();

//From application
builder.Services.AddServices();

builder.Services.AddSession(opts =>
{
    opts.IdleTimeout = TimeSpan.FromMinutes(30);
    opts.Cookie.IsEssential = true;
});

//Serilog
builder.Host.UseSerilog((context, configuration) => configuration.ReadFrom.Configuration(context.Configuration));

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var app = builder.Build();


app.UseSession();

app.MapRazorPages();

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

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<RefreshTokenMiddleware>();

//Чтобы сервис включался при запуске (добавляем автоматически роли и админа если этого нет в БД)
var scope = app.Services.CreateScope();
var service = scope.ServiceProvider.GetService<Initializer>();
service.Initialize();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


app.Run();