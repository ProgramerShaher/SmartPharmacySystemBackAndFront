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
    private readonly TimeSpan _checkInterval = TimeSpan.FromHours(24);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("ExpiryCheckWorker is starting.");

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
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var medicineBatchService = scope.ServiceProvider.GetRequiredService<IMedicineBatchService>();

        var now = DateTime.UtcNow.Date;
        var thresholdDate = now.AddDays(30);

        // 1. Get batches that are either expired or near expiry
        var batchesToCheck = await unitOfWork.MedicineBatches.GetAllAsync(); 
        var criticalBatches = batchesToCheck.Where(b => !b.IsDeleted && b.Status == "Active" && b.ExpiryDate.Date <= now).ToList();
        var warningBatches = batchesToCheck.Where(b => !b.IsDeleted && b.Status == "Active" && b.ExpiryDate.Date > now && b.ExpiryDate.Date <= thresholdDate).ToList();

        // 2. Process Critical (Expired) -> Trigger Full Financial Loss (Auto-Scrapping)
        foreach (var batch in criticalBatches)
        {
            try
            {
                logger.LogInformation("Batch {Barcode} is expired. Triggering automatic financial loss/scraping.", batch.BatchBarcode);
                await medicineBatchService.ProcessFinancialLossAsync(batch.Id);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to process automatic financial loss for batch {BatchId}", batch.Id);
            }
        }

        // 3. Process Warning (Near Expiry)
        foreach (var batch in warningBatches)
        {
            var daysLeft = (batch.ExpiryDate.Date - now).Days;
            var severity = daysLeft <= 7 ? AlertSeverity.Warning : AlertSeverity.Info;

            var alert = new Alert(
                batch.Id,
                AlertType.ExpiryTwoWeeks,
                severity,
                $"الصنف {batch.Medicine?.Name} (باركود: {batch.BatchBarcode}) سينتهي خلال {daysLeft} أيام.",
                batch.ExpiryDate
            );

            await unitOfWork.Alerts.AddAsync(alert);
        }

        if (criticalBatches.Any() || warningBatches.Any())
        {
            await unitOfWork.SaveChangesAsync();
            logger.LogInformation("Processed {Critical} critical and {Warning} warning expiries.", criticalBatches.Count, warningBatches.Count);
        }
    }
}
