using SmartPharmacySystem.Application.DTOs.Dashboard;

namespace SmartPharmacySystem.Application.Interfaces;

/// <summary>
/// Master Dashboard Service - Single Source of Truth
/// Provides comprehensive real-time statistics for executive management
/// </summary>
public interface IMasterDashboardService
{
    /// <summary>
    /// Get all master dashboard statistics in a single optimized query
    /// Target response time: <100ms
    /// </summary>
    /// <returns>Comprehensive dashboard statistics</returns>
    Task<MasterDashboardStatsDto> GetMasterDashboardStatsAsync();
}
