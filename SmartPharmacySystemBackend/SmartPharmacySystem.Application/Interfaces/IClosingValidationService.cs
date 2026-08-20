using System;
using System.Threading.Tasks;

namespace SmartPharmacySystem.Application.Interfaces;

/// <summary>
/// Service responsible for validating if a specific date is closed/locked.
/// This acts as a firewall to prevent any financial modifications in closed periods.
/// </summary>
public interface IClosingValidationService
{
    /// <summary>
    /// Throws an InvalidOperationException if the given date is closed by a Financial Period or a Daily Closing.
    /// </summary>
    Task ValidateDateIsUnlockedAsync(DateTime date, int? branchId = null);
    
    /// <summary>
    /// Checks if the given date is closed without throwing an exception.
    /// </summary>
    Task<bool> IsDateClosedAsync(DateTime date, int? branchId = null);
}
