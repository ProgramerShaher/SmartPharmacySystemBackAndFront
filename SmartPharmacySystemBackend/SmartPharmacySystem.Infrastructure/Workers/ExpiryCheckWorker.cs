using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SmartPharmacySystem.Infrastructure.Hubs;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Enums;
using SmartPharmacySystem.Core.Interfaces;
using SmartPharmacySystem.Application.Interfaces;

namespace SmartPharmacySystem.Infrastructure.Workers;

/// <summary>
/// Background worker that checks for medicine batch expiry every 24 hours.
/// Uses .NET 9 PeriodicTimer for efficient scheduling.
/// </summary>
public class ExpiryCheckWorker(
    IServiceProvider serviceProvider,
    ILogger<ExpiryCheckWorker> logger) : BackgroundService
{
    private readonly TimeSpan _checkInterval = TimeSpan.FromHours(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("ExpiryCheckWorker is starting (Interval: 1 hour).");

        using PeriodicTimer timer = new(_checkInterval);

        try
        {
            // Initial check when service starts
            await CheckExpiriesAsync();

            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await CheckExpiriesAsync();
            }
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("ExpiryCheckWorker is stopping.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in ExpiryCheckWorker.");
        }
    }

    private async Task CheckExpiriesAsync()
    {
        logger.LogInformation("Scanning for expired/expiring batches at: {Time}", DateTimeOffset.Now);

        using var scope = serviceProvider.CreateScope();
        var alertService = scope.ServiceProvider.GetRequiredService<IAlertService>();
        
        await alertService.GenerateExpiryAlertsAsync();
        await alertService.GenerateLowStockAlertsAsync();
    }
}
