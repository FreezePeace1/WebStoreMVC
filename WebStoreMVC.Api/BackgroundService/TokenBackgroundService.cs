using Microsoft.AspNetCore.Identity;
using WebStoreMVC.Domain.Entities;
using WebStoreMVC.Services.Interfaces;
using ILogger = Serilog.ILogger;

namespace WebStoreMVC.BackgroundService;

public class TokenBackgroundService : Microsoft.Extensions.Hosting.BackgroundService
{
    private readonly ILogger _logger;
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public TokenBackgroundService(ILogger logger,
        IHttpContextAccessor contextAccessor, IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger;
        _contextAccessor = contextAccessor;
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RefreshAccessToken();
            }
            catch (Exception e)
            {
                _logger.Error(e, e.Message);
            }

            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }

    private async Task RefreshAccessToken()
    {
        
    }
}