using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SmartPharmacySystem.Application.IServices;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Enums;
using SmartPharmacySystem.Core.Interfaces;

namespace SmartPharmacySystem.Infrastructure.Workers;

public class BackupSchedulerWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<BackupSchedulerWorker> _logger;

    public BackupSchedulerWorker(IServiceProvider serviceProvider, ILogger<BackupSchedulerWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Backup Scheduler Worker is starting.");
        
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));

        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await DoWorkAsync();
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Backup Scheduler Worker is stopping.");
        }
    }

    private async Task DoWorkAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var backupService = scope.ServiceProvider.GetRequiredService<IBackupService>();
        var stateService = scope.ServiceProvider.GetRequiredService<IBackupStateService>();
        
        var config = await unitOfWork.BackupConfigurations.GetConfigurationAsync();
        
        if (config == null || !config.IsAutoBackupEnabled)
        {
            return;
        }

        if (config.NextScheduledRun == null)
        {
            // Initialize NextScheduledRun if it's the first time
            var nextRun = CalculateNextRun(config);
            await stateService.UpdateNextScheduledRunAsync(config, nextRun);
            return;
        }

        if (DateTime.UtcNow >= config.NextScheduledRun.Value)
        {
            // Time to run!
            _logger.LogInformation("Scheduled backup time reached. Initiating backup.");
            
            var timezoneInfo = TimeZoneInfo.FindSystemTimeZoneById(config.Timezone);
            var localTime = TimeZoneInfo.ConvertTimeFromUtc(config.NextScheduledRun.Value, timezoneInfo);
            var occurrenceId = $"Auto_{localTime:yyyyMMdd_HHmmss}_UTC";

            // Execute the backup completely in background (it has its own lock)
            // Await it so we don't proceed with updating the schedule until it finishes or fails
            await backupService.ExecuteBackupAsync(BackupType.Automatic, occurrenceId);

            // Fetch the history to see if it completed successfully or failed terminally
            var history = await unitOfWork.BackupHistories.GetByOccurrenceIdAsync(occurrenceId);
            
            if (history != null && history.Status == BackupStatus.Completed)
            {
                // Only advance schedule if completed
                var nextRun = CalculateNextRun(config);
                await stateService.UpdateNextScheduledRunAsync(config, nextRun);
            }
        }
    }

    private DateTime CalculateNextRun(BackupConfiguration config)
    {
        var tz = TimeZoneInfo.FindSystemTimeZoneById(config.Timezone);
        var nowLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
        
        // Find the scheduled time for today
        var todayScheduledLocal = nowLocal.Date.Add(config.ScheduledTime);
        
        DateTime nextLocal;

        if (todayScheduledLocal > nowLocal)
        {
            // Scheduled time hasn't passed yet today
            nextLocal = todayScheduledLocal;
        }
        else
        {
            // Scheduled time has passed today, move to the next interval
            var daysToAdd = config.Frequency switch
            {
                BackupFrequency.Daily => 1,
                BackupFrequency.Every2Days => 2,
                BackupFrequency.Every3Days => 3,
                BackupFrequency.Weekly => 7,
                BackupFrequency.Monthly => DateTime.DaysInMonth(todayScheduledLocal.Year, todayScheduledLocal.Month),
                BackupFrequency.Custom => config.IntervalDays,
                _ => 1
            };
            
            nextLocal = todayScheduledLocal.AddDays(daysToAdd);
        }

        return TimeZoneInfo.ConvertTimeToUtc(nextLocal, tz);
    }
}
