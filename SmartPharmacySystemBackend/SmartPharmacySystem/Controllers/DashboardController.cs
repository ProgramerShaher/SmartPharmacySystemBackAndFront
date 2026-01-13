using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartPharmacySystem.Application.DTOs.Dashboard;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Application.Wrappers;

namespace SmartPharmacySystem.Controllers;

/// <summary>
/// Master Dashboard Controller - Single Source of Truth
/// Provides unified endpoint for all executive dashboard statistics
/// </summary>
[Route("api/[controller]")]
[Authorize]
[ApiController]
public class DashboardController : ControllerBase
{
    private readonly IMasterDashboardService _masterDashboardService;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(
        IMasterDashboardService masterDashboardService,
        ILogger<DashboardController> logger)
    {
        _masterDashboardService = masterDashboardService;
        _logger = logger;
    }

    /// <summary>
    /// Get Master Dashboard Statistics
    /// Returns comprehensive real-time statistics for:
    /// - Financial Intelligence (Net Profit, Liquidity, Net Debt, Cash Flow)
    /// - Inventory Intelligence (Value, Expiry Radar, Critical Stock)
    /// - Operational Pulse (Activity Stream, Cashier Performance, Heat Map)
    /// 
    /// Optimized for <100ms response time using parallel async queries
    /// </summary>
    /// <returns>Master dashboard statistics</returns>
    [HttpGet("master-stats")]
    public async Task<IActionResult> GetMasterStats()
    {
        _logger.LogInformation("[Dashboard API] Master stats endpoint called");
        
        try
        {
            _logger.LogInformation("[Dashboard API] Calling service...");
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            var stats = await _masterDashboardService.GetMasterDashboardStatsAsync();
            
            stopwatch.Stop();
            _logger.LogInformation("[Dashboard API] Master stats returned in {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);

            return Ok(ApiResponse<MasterDashboardStatsDto>.Succeeded(
                stats, 
                $"تم جلب إحصائيات لوحة التحكم بنجاح ({stopwatch.ElapsedMilliseconds}ms)"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Dashboard API] CRITICAL ERROR: {Message}", ex.Message);
            _logger.LogError(ex, "[Dashboard API] Stack Trace: {StackTrace}", ex.StackTrace);
            
            if (ex.InnerException != null)
            {
                _logger.LogError(ex.InnerException, "[Dashboard API] Inner Exception: {Message}", ex.InnerException.Message);
            }
            
            return StatusCode(500, ApiResponse<object>.Failed($"خطأ داخلي: {ex.Message}"));
        }
    }
}
