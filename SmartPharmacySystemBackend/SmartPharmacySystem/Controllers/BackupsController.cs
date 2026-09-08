using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartPharmacySystem.Application.DTOs;
using SmartPharmacySystem.Application.DTOs.Backup;
using SmartPharmacySystem.Application.DTOs.Shared;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Application.IServices;
using SmartPharmacySystem.Application.Wrappers;
using SmartPharmacySystem.Authorization;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Enums;
using SmartPharmacySystem.Core.Interfaces;
using SmartPharmacySystem.Infrastructure.Workers;

namespace SmartPharmacySystem.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
[RequirePermission("admin.backup.manage")]
public class BackupsController : ControllerBase
{
    private readonly IBackupService _backupService;
    private readonly IBackupStateService _backupStateService;
    private readonly IKeyManagementService _keyManagementService;
    private readonly IGoogleDriveService _googleDriveService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthService _authService;
    private readonly ICurrentUserService _currentUserService;

    public BackupsController(
        IBackupService backupService,
        IBackupStateService backupStateService,
        IKeyManagementService keyManagementService,
        IGoogleDriveService googleDriveService,
        IUnitOfWork unitOfWork,
        IAuthService authService,
        ICurrentUserService currentUserService)
    {
        _backupService = backupService;
        _backupStateService = backupStateService;
        _keyManagementService = keyManagementService;
        _googleDriveService = googleDriveService;
        _unitOfWork = unitOfWork;
        _authService = authService;
        _currentUserService = currentUserService;
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<ApiResponse<BackupDashboardDto>>> GetDashboard()
    {
        var config = await _unitOfWork.BackupConfigurations.GetConfigurationAsync();
        var total = await _unitOfWork.BackupHistories.GetTotalCountAsync();
        var successful = await _unitOfWork.BackupHistories.GetByStatusAsync(BackupStatus.Completed);
        var failed = await _unitOfWork.BackupHistories.GetByStatusAsync(BackupStatus.Failed);

        var activeStates = new[] { BackupStatus.Pending, BackupStatus.SqlBackupRunning, BackupStatus.VerificationRunning, BackupStatus.EncryptionRunning, BackupStatus.Uploading, BackupStatus.Retrying };
        var activeBackupEntity = await _unitOfWork.BackupHistories.GetAllAsync(1, 10);
        var activeBackup = activeBackupEntity.FirstOrDefault(h => activeStates.Contains(h.Status));

        var dto = new BackupDashboardDto
        {
            IsSubsystemHealthy = BackupStartupTask.IsSqlBackupHealthy,
            SubsystemHealthMessage = BackupStartupTask.SqlBackupHealthMessage,
            TotalBackups = total,
            SuccessfulBackups = successful.Count(),
            FailedBackups = failed.Count(),
            LastSuccessfulBackup = successful.OrderByDescending(x => x.CompletedAt).FirstOrDefault()?.CompletedAt is DateTime c ? DateTime.SpecifyKind(c, DateTimeKind.Utc) : null,
            NextScheduledBackup = config?.IsAutoBackupEnabled == true && config.NextScheduledRun.HasValue ? DateTime.SpecifyKind(config.NextScheduledRun.Value, DateTimeKind.Utc) : null,
            ActiveBackup = activeBackup != null ? MapToDto(activeBackup) : null
        };

        return Ok(ApiResponse<BackupDashboardDto>.Succeeded(dto, "تم جلب البيانات بنجاح"));
    }

    [HttpGet("settings")]
    public async Task<ActionResult<ApiResponse<BackupConfigDto>>> GetSettings()
    {
        var config = await _unitOfWork.BackupConfigurations.GetConfigurationAsync();
        if (config == null)
        {
            config = new BackupConfiguration(); // Return defaults
        }

        var dto = new BackupConfigDto
        {
            IsAutoBackupEnabled = config.IsAutoBackupEnabled,
            Frequency = config.Frequency,
            IntervalDays = config.IntervalDays,
            ScheduledTime = config.ScheduledTime,
            Timezone = config.Timezone,
            NextScheduledRun = config.NextScheduledRun.HasValue ? DateTime.SpecifyKind(config.NextScheduledRun.Value, DateTimeKind.Utc) : null,
            RetentionMode = config.RetentionMode,
            MaxBackupsToKeep = config.MaxBackupsToKeep,
            RetentionDays = config.RetentionDays,
            LocalRetentionDays = config.LocalRetentionDays,
            GoogleDriveFolderId = config.GoogleDriveFolderId,
            GoogleDriveFolderName = config.GoogleDriveFolderName,
            VerifyAfterBackup = config.VerifyAfterBackup,
            EncryptBackup = config.EncryptBackup
        };

        return Ok(ApiResponse<BackupConfigDto>.Succeeded(dto, "تم جلب الإعدادات بنجاح"));
    }

    [HttpPut("settings")]
    public async Task<ActionResult<ApiResponse<BackupConfigDto>>> UpdateSettings(UpdateBackupConfigDto request)
    {
        var config = await _unitOfWork.BackupConfigurations.GetConfigurationAsync() ?? new BackupConfiguration();
        
        config.IsAutoBackupEnabled = request.IsAutoBackupEnabled;
        config.Frequency = request.Frequency;
        config.IntervalDays = request.IntervalDays;
        config.ScheduledTime = request.ScheduledTime;
        config.Timezone = request.Timezone;
        config.RetentionMode = request.RetentionMode;
        config.MaxBackupsToKeep = request.MaxBackupsToKeep;
        config.RetentionDays = request.RetentionDays;
        config.LocalRetentionDays = request.LocalRetentionDays;
        config.GoogleDriveFolderId = request.GoogleDriveFolderId;
        config.GoogleDriveFolderName = request.GoogleDriveFolderName;
        config.VerifyAfterBackup = request.VerifyAfterBackup;
        config.EncryptBackup = request.EncryptBackup;

        if (request.IsAutoBackupEnabled)
        {
            // Recalculate next run
            var tz = TimeZoneInfo.FindSystemTimeZoneById(request.Timezone);
            var nowLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
            var todayScheduledLocal = nowLocal.Date.Add(request.ScheduledTime);
            
            DateTime nextLocal = todayScheduledLocal > nowLocal ? todayScheduledLocal : todayScheduledLocal.AddDays(
                request.Frequency == BackupFrequency.Daily ? 1 : 
                request.Frequency == BackupFrequency.Every2Days ? 2 : 
                request.Frequency == BackupFrequency.Every3Days ? 3 : 
                request.Frequency == BackupFrequency.Weekly ? 7 : 
                request.Frequency == BackupFrequency.Monthly ? DateTime.DaysInMonth(todayScheduledLocal.Year, todayScheduledLocal.Month) : 
                request.IntervalDays);
                
            config.NextScheduledRun = TimeZoneInfo.ConvertTimeToUtc(nextLocal, tz);
        }
        else
        {
            config.NextScheduledRun = null;
        }

        await _unitOfWork.BackupConfigurations.UpdateConfigurationAsync(config);
        
        // Ensure Key is initialized if encryption is enabled
        if (config.EncryptBackup)
        {
            try
            {
                var currentKey = _keyManagementService.LoadKey();
                if (currentKey == null)
                {
                    _keyManagementService.InitializeNewKey();
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<BackupConfigDto>.Failed($"تم حفظ الإعدادات، لكن تعذر تهيئة مفتاح التشفير: {ex.Message}"));
            }
        }

        return await GetSettings();
    }

    [HttpGet("history")]
    public async Task<ActionResult<ApiResponse<PagedResult<BackupHistoryDto>>>> GetHistory([FromQuery] BackupHistoryFilterDto filter)
    {
        var allRecords = await _unitOfWork.BackupHistories.GetAllAsync(1, 1000); // Simple approach for now
        var query = allRecords.AsQueryable();

        if (filter.Status.HasValue)
            query = query.Where(h => h.Status == filter.Status.Value);
        
        if (filter.Type.HasValue)
            query = query.Where(h => h.BackupType == filter.Type.Value);

        var total = query.Count();
        var pagedData = query.Skip((filter.Page - 1) * filter.PageSize).Take(filter.PageSize).ToList();

        var result = new PagedResult<BackupHistoryDto>(
            pagedData.Select(MapToDto).ToList(),
            total,
            filter.Page,
            filter.PageSize
        );

        return Ok(ApiResponse<PagedResult<BackupHistoryDto>>.Succeeded(result, "تم جلب السجل بنجاح"));
    }

    [HttpPost("manual")]
    public async Task<ActionResult<ApiResponse<BackupHistoryDto>>> TriggerManualBackup()
    {
        var userId = _currentUserService.UserId ?? 0;
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        var userName = user?.FullName ?? "Unknown Admin";

        try
        {
            var history = await _backupService.TriggerManualBackupAsync(userId, userName);
            return Ok(ApiResponse<BackupHistoryDto>.Succeeded(MapToDto(history), "تم بدء النسخ الاحتياطي اليدوي."));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<BackupHistoryDto>.Failed(ex.Message));
        }
    }

    [HttpPost("retry/{id}")]
    public async Task<ActionResult<ApiResponse<BackupHistoryDto>>> RetryBackup(int id)
    {
        var userId = _currentUserService.UserId ?? 0;
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        var userName = user?.FullName ?? "Unknown Admin";

        try
        {
            var history = await _backupService.RetryBackupAsync(id, userId, userName);
            return Ok(ApiResponse<BackupHistoryDto>.Succeeded(MapToDto(history), "تم بدء إعادة المحاولة."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<BackupHistoryDto>.Failed(ex.Message));
        }
    }

    [HttpPost("settings/export-key")]
    [RequirePermission("admin.backup.export_key")]
    public async Task<ActionResult<ApiResponse<ExportKeyResponseDto>>> ExportKey(ExportKeyDto request)
    {
        var userId = _currentUserService.UserId ?? 0;
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        
        if (user == null || !_authService.VerifyPassword(request.CurrentPassword, user.PasswordHash))
        {
            return BadRequest(ApiResponse<ExportKeyResponseDto>.Failed("كلمة المرور الحالية غير صحيحة."));
        }

        var base64Key = _keyManagementService.ExportKeyBase64();
        if (base64Key == null)
        {
            return BadRequest(ApiResponse<ExportKeyResponseDto>.Failed("لا يوجد مفتاح استرداد لتصديره. يرجى تهيئة النظام أولاً."));
        }

        return Ok(ApiResponse<ExportKeyResponseDto>.Succeeded(new ExportKeyResponseDto { Base64Key = base64Key }, "تم تصدير المفتاح بنجاح"));
    }

    [HttpPost("settings/import-key")]
    [RequirePermission("admin.backup.export_key")]
    public async Task<ActionResult<ApiResponse<string>>> ImportKey(ImportKeyDto request)
    {
        var userId = _currentUserService.UserId ?? 0;
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        
        if (user == null || !_authService.VerifyPassword(request.CurrentPassword, user.PasswordHash))
        {
            return BadRequest(ApiResponse<string>.Failed("كلمة المرور الحالية غير صحيحة."));
        }

        try
        {
            _keyManagementService.ImportKeyBase64(request.Base64Key);
            return Ok(ApiResponse<string>.Succeeded("تم", "تم استيراد وحفظ المفتاح بنجاح."));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<string>.Failed(ex.Message));
        }
    }

    [HttpPost("settings/test-drive")]
    public async Task<ActionResult<ApiResponse<DriveConnectionTestDto>>> TestDriveConnection()
    {
        var (isSuccess, message) = await _googleDriveService.TestConnectionAsync();
        
        var dto = new DriveConnectionTestDto 
        { 
            IsSuccess = isSuccess, 
            Message = message 
        };
        
        return Ok(ApiResponse<DriveConnectionTestDto>.Succeeded(dto, "اكتمل فحص الاتصال"));
    }

    private static BackupHistoryDto MapToDto(BackupHistory entity)
    {
        return new BackupHistoryDto
        {
            Id = entity.Id,
            OccurrenceId = entity.OccurrenceId,
            DatabaseName = entity.DatabaseName,
            BackupType = entity.BackupType,
            Status = entity.Status,
            UploadStatus = entity.UploadStatus,
            IsRecovery = entity.IsRecovery,
            StartedAt = entity.StartedAt,
            CompletedAt = entity.CompletedAt,
            DurationSeconds = entity.DurationSeconds,
            FileName = entity.FileName,
            FileSizeBytes = entity.FileSizeBytes,
            IsVerified = entity.IsVerified,
            VerificationMessage = entity.VerificationMessage,
            GoogleDriveWebViewLink = entity.GoogleDriveWebViewLink,
            ErrorMessage = entity.ErrorMessage,
            TriggeredByUserName = entity.TriggeredByUserName,
            RetryCount = entity.RetryCount
        };
    }
}
