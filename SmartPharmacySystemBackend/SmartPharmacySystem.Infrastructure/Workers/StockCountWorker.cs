using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SmartPharmacySystem.Application.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SmartPharmacySystem.Infrastructure.Workers;

/// <summary>
/// Background worker that checks for scheduled stock counts every 24 hours.
/// Uses .NET PeriodicTimer for efficient scheduling.
/// </summary>
public class StockCountWorker(
    IServiceProvider serviceProvider,
    ILogger<StockCountWorker> logger) : BackgroundService
{
    private readonly TimeSpan _checkInterval = TimeSpan.FromHours(24);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("StockCountWorker is starting (Interval: 24 hours).");

        // Calculate time until next midnight to run the first check
        var now = DateTime.Now;
        var nextMidnight = now.Date.AddDays(1);
        var initialDelay = nextMidnight - now;

        // Wait until midnight for the first run
        await Task.Delay(initialDelay, stoppingToken);

        using PeriodicTimer timer = new(_checkInterval);

        try
        {
            // Initial check when timer hits
            await ProcessSchedulesAsync();

            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await ProcessSchedulesAsync();
            }
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("StockCountWorker is stopping.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in StockCountWorker.");
        }
    }

    private async Task ProcessSchedulesAsync()
    {
        logger.LogInformation("Scanning for due stock count schedules at: {Time}", DateTimeOffset.Now);

        using var scope = serviceProvider.CreateScope();
        var stockCountService = scope.ServiceProvider.GetRequiredService<IStockCountService>();
        
        await stockCountService.ProcessScheduledCountsAsync();
    }
}
