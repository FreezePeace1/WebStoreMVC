using System.Net;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using Microsoft.OpenApi.Models;
using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Nest;
using Newtonsoft.Json.Serialization;
using WebStoreMVC.DAL.Context;
using WebStoreMVC.Domain.Entities;
using WebStoreMVC.Policy;

namespace WebStoreMVC;

public static class Startup
{
    /// <summary>
    /// Подключение и настройка Identity
    /// </summary>
    /// <param name="services"></param>
    public static void AddIdentity(this IServiceCollection services)
    {
        
        //Подключаем Identity
        services.AddIdentity<AppUser, IdentityRole>( /*options => options.SignIn.RequireConfirmedAccount = true*/)
            .AddEntityFrameworkStores<WebStoreContext>()
            .AddDefaultTokenProviders();

        //Настраиваем Identity
        services.Configure<IdentityOptions>(opt =>
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

            //Нужен нормальный email (существующий)
            opt.User.RequireUniqueEmail = true;
        });
        services.ConfigureApplicationCookie(opt =>
        {
            /*opt.Cookie.Name = "WebStoreMvc_Cookie";
            opt.Cookie.HttpOnly = true;
            opt.ExpireTimeSpan = TimeSpan.FromDays(30);*/
            opt.LoginPath = "/Account/Login";
            opt.LogoutPath = "/Account/Logout";
            opt.AccessDeniedPath = "/Home/";

            //Чтобы состояние пользователя во время сессии менялось (например если он зарегистрировался или залогинился то сразу переключаем на эти права)
            /*opt.SlidingExpiration = true;*/
            
        });
    }

    /// <summary>
    /// Добавление swagger
    /// </summary>
    /// <param name="services"></param>
    public static void AddSwagger(this IServiceCollection services)
    {
        services.AddApiVersioning()
            .AddApiExplorer(opt =>
            {
                opt.DefaultApiVersion = new ApiVersion(1, 0);
                opt.GroupNameFormat = "'v'VVV";
                opt.SubstituteApiVersionInUrl = true;
                opt.AssumeDefaultVersionWhenUnspecified = true;
            });
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(opt =>
        {
            opt.SwaggerDoc("v1", new OpenApiInfo()
            {
                Version = "v1",
                Title = "Diary.Api",
                Description = "Version 1.0",
                //Adding uri
                /*TermsOfService = new Uri()*/
            });

            opt.SwaggerDoc("v2", new OpenApiInfo()
            {
                Version = "v2",
                Title = "Diary.Api",
                Description = "Version 2.0",
                //Adding uri
                /*TermsOfService = new Uri()*/
            });

            opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
            {
                In = ParameterLocation.Header,
                Description = "Enter valid token",
                Name = "Authorize",
                Type = SecuritySchemeType.Http,
                BearerFormat = "JWT",
                Scheme = "Bearer"
            });

            opt.AddSecurityRequirement(new OpenApiSecurityRequirement()
            {
                {
                    new OpenApiSecurityScheme()
                    {
                        Reference = new OpenApiReference()
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        },
                        Name = "Bearer",
                        In = ParameterLocation.Header
                    },
                    Array.Empty<string>()
                }
            });
            var xmlFileName = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            opt.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFileName));
        });
    }

    /// <summary>
    /// Добавление аутентификации и авторизации с помощью JWT
    /// </summary>
    /// <param name="services"></param>
    /// <param name="builder"></param>
    public static void AddAuthenticationAndAuthorization(this IServiceCollection services,
        WebApplicationBuilder builder)
    {
        /*services  
            .AddAuthentication() // Cookie by default  
            .AddCookie(options =>  
            {  
                options.LoginPath = "/Account/Unauthorized/";  
                options.AccessDeniedPath = "/Account/Forbidden/";  
            })  
            .AddJwtBearer(opt =>  
            {  
                opt.SaveToken = true;
                opt.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
                    ValidAudience = builder.Configuration["JwtSettings:Audience"],
                    IssuerSigningKey =
                        new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Key"]))
                }; 
                
            });  */
        //Подключаем JWT
        services.AddAuthentication(opt =>
            {
                opt.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                /*opt.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;*/
            })
            .AddCookie(opt =>
            {
                opt.Cookie.Name = "WebStoreMvc_Cookie";
                opt.Cookie.HttpOnly = true;
                opt.ExpireTimeSpan = TimeSpan.FromDays(30);
                opt.LoginPath = "/Account/Login";
                opt.LogoutPath = "/Account/Logout";
                /*opt.AccessDeniedPath = "/Home/Index";*/
            })
            .AddJwtBearer("Bearer", opt =>
            {
                opt.SaveToken = true;
                opt.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
                    ValidAudience = builder.Configuration["JwtSettings:Audience"],
                    IssuerSigningKey =
                        new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Key"])),
                    
                    RoleClaimType = UserRoles.ADMINISTRATOR
                };
            });

        //Для добавления Policy
        services.AddHttpContextAccessor();
        services.AddSingleton<IAuthorizationHandler, CookieRequirementHandler>();

        services.AddAuthorization(options =>
        {
            var defaultAuthorizationPolicyBuilder = new AuthorizationPolicyBuilder(
                JwtBearerDefaults.AuthenticationScheme,
                CookieAuthenticationDefaults.AuthenticationScheme,
                "Bearer");
            defaultAuthorizationPolicyBuilder =
                defaultAuthorizationPolicyBuilder.RequireAuthenticatedUser();
            options.DefaultPolicy = defaultAuthorizationPolicyBuilder.Build();

            options.AddPolicy("Default", new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build());

            options.AddPolicy("AdminCookie", policy =>
            {
                policy.Requirements.Add(new CookieAdminRequirement());
                policy.RequireAuthenticatedUser();
                policy.RequireRole(UserRoles.ADMINISTRATOR);
            });

            options.AddPolicy("UserCookie", policy =>
            {
                policy.Requirements.Add(new CookieUserRequirement());
                policy.RequireAuthenticatedUser();
            });
        });
        
    }

    public static void AddElasticsearch(this IServiceCollection services, IConfiguration configuration)
    {
        var baseUrl = configuration["ElasticSettings:baseUrl"];
        var index = configuration["ElasticSettings:defaultIndex"];
        var settings = new ConnectionSettings(new Uri(baseUrl ?? "")).PrettyJson().CertificateFingerprint("6b6a8c2ad2bc7b291a7363f7bb96a120b8de326914980c868c1c0bc6b3dc41fd")
            .BasicAuthentication("elastic", "JbNb_unwrJy3W0OaZ07n=").DefaultIndex(index);
        settings.EnableApiVersioningHeader();
        AddDefaultMappings(settings);
        var client = new ElasticClient(settings);
        services.AddSingleton<IElasticClient>(client);
        CreateIndex(client,index);

    }
    private static void AddDefaultMappings(ConnectionSettings settings) {
        settings.DefaultMappingFor<Product>(m => m.
            Ignore(p => p.ProductId)
            .Ignore(p => p.Images)
            .Ignore(p => p.Price)
            .Ignore(p => p.Quantity)
            .Ignore(p => p.Article));
    }
    private static void CreateIndex(IElasticClient client, string indexName) {
        var createIndexResponse = client.Indices.Create(indexName, index => index.Map<Product>(x => x.AutoMap()));
    }
}