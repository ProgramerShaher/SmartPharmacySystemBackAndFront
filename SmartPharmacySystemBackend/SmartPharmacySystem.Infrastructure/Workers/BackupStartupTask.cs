using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SmartPharmacySystem.Application.IServices;

namespace SmartPharmacySystem.Infrastructure.Workers;

public class BackupStartupTask : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<BackupStartupTask> _logger;
    private readonly IConfiguration _configuration;

    public static bool IsSqlBackupHealthy { get; private set; } = true;
    public static string SqlBackupHealthMessage { get; private set; } = "صحي (Healthy)";

    public BackupStartupTask(IServiceProvider serviceProvider, ILogger<BackupStartupTask> logger, IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Backup Startup Task is executing...");

        using var scope = _serviceProvider.CreateScope();
        
        // 1. Mark interrupted backups as failed
        var stateService = scope.ServiceProvider.GetRequiredService<IBackupStateService>();
        await stateService.MarkInterruptedBackupsAsFailedAsync();

        // 2. Cleanup orphaned local files
        var retentionService = scope.ServiceProvider.GetRequiredService<IBackupRetentionService>();
        await retentionService.CleanupLocalFilesAsync();

        // 3. Check SQL Server Backup Permissions
        await CheckSqlBackupPermissionsAsync();
    }

    private async Task CheckSqlBackupPermissionsAsync()
    {
        try
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync();

            var sql = "SELECT has_perms_by_name(DB_NAME(), 'DATABASE', 'BACKUP DATABASE');";
            using var cmd = new SqlCommand(sql, conn);
            var result = await cmd.ExecuteScalarAsync();

            if (result != null && result != DBNull.Value)
            {
                var hasPermission = Convert.ToInt32(result);
                if (hasPermission == 1)
                {
                    IsSqlBackupHealthy = true;
                    SqlBackupHealthMessage = "صلاحيات النسخ الاحتياطي في قاعدة البيانات متوفرة.";
                    _logger.LogInformation(SqlBackupHealthMessage);
                }
                else
                {
                    IsSqlBackupHealthy = false;
                    SqlBackupHealthMessage = "تحذير: لا يملك حساب الاتصال بقاعدة البيانات صلاحية BACKUP DATABASE.";
                    _logger.LogWarning(SqlBackupHealthMessage);
                }
            }
            else
            {
                IsSqlBackupHealthy = false;
                SqlBackupHealthMessage = "تعذر تحديد صلاحية BACKUP DATABASE.";
                _logger.LogWarning(SqlBackupHealthMessage);
            }
        }
        catch (Exception ex)
        {
            IsSqlBackupHealthy = false;
            SqlBackupHealthMessage = $"خطأ أثناء فحص صلاحيات النسخ الاحتياطي: {ex.Message}";
            _logger.LogError(ex, SqlBackupHealthMessage);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
