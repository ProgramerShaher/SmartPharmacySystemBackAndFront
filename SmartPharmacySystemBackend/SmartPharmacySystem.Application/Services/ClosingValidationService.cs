using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Application.Interfaces.Data;
using SmartPharmacySystem.Core.Interfaces;
using SmartPharmacySystem.Core.Enums;
using Microsoft.Extensions.Logging;
using System.Linq;
using SmartPharmacySystem.Core.Entities;

namespace SmartPharmacySystem.Application.Services;

public class ClosingValidationService : IClosingValidationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IApplicationDbContext _context;
    private readonly ILogger<ClosingValidationService> _logger;

    public ClosingValidationService(
        IUnitOfWork unitOfWork,
        IApplicationDbContext context,
        ILogger<ClosingValidationService> logger)
    {
        _unitOfWork = unitOfWork;
        _context = context;
        _logger = logger;
    }

    public async Task ValidateDateIsUnlockedAsync(DateTime date, int? branchId = null)
    {
        var localDate = date.Kind == DateTimeKind.Utc ? date.ToLocalTime() : date;
        var isClosed = await IsDateClosedAsync(localDate, branchId);
        if (isClosed)
        {
            throw new InvalidOperationException($"التاريخ {localDate:yyyy-MM-dd} مقفل محاسبياً (إقفال يومي أو فتري). لا يمكن إجراء أو تعديل أي عملية في تاريخ مقفل.");
        }
    }

    public async Task<bool> IsDateClosedAsync(DateTime date, int? branchId = null)
    {
        var localDate = date.Kind == DateTimeKind.Utc ? date.ToLocalTime() : date;
        var dateOnly = localDate.Date;

        // 1. Check Financial Period Lock (Month/Year)
        var financialPeriod = await _unitOfWork.FinancialPeriods.GetByDateAsync(localDate);
        if (financialPeriod != null && financialPeriod.IsClosed)
        {
            _logger.LogWarning("Validation Failed: Attempted to post in closed financial period {PeriodName}", financialPeriod.PeriodName);
            return true;
        }

        // 2. Check Daily Closing Lock
        var dailyClosingQuery = _context.DailyClosings.AsNoTracking().Where(d => d.ClosingDate.Date == dateOnly && d.Status == ClosingStatus.Approved);
        
        if (branchId.HasValue)
        {
            dailyClosingQuery = dailyClosingQuery.Where(d => d.BranchId == branchId.Value);
        }

        var isDailyClosed = await dailyClosingQuery.AnyAsync();
        
        if (isDailyClosed)
        {
            _logger.LogWarning("Validation Failed: Attempted to post in closed day {Date}", dateOnly);
            return true;
        }

        return false;
    }
}
