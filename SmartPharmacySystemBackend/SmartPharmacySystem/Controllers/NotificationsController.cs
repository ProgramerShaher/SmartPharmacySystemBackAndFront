using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartPharmacySystem.Application.DTOs.Notifications;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Application.Wrappers;

namespace SmartPharmacySystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly IAlertService _alertService;

        public NotificationsController(IAlertService alertService)
        {
            _alertService = alertService;
        }

        /// <summary>
        /// Get real-time medicine expiry alerts
        /// </summary>
        /// <returns>List of expiry alerts sorted by criticality</returns>
        [HttpGet("expiry-alerts")]
        public async Task<IActionResult> GetExpiryAlerts()
        {
            var alerts = await _alertService.GetRealTimeExpiryAlertsAsync();
            return Ok(ApiResponse<IEnumerable<ExpiryAlertDto>>.Succeeded(alerts, "تم جلب تنبيهات الصلاحية بنجاح"));
        }

        /// <summary>
        /// Get unified system alerts (Expiry & Low Stock)
        /// </summary>
        /// <returns>List of unified alerts with colors and levels</returns>
        [HttpGet("unified-alerts")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<SmartPharmacySystem.Application.DTOs.Alerts.UnifiedAlertDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUnifiedAlerts()
        {
            var alerts = await _alertService.GetActiveSystemAlertsAsync();
            return Ok(ApiResponse<IEnumerable<SmartPharmacySystem.Application.DTOs.Alerts.UnifiedAlertDto>>.Succeeded(alerts, "تم جلب التنبيهات الموحدة بنجاح"));
        }
    }
}
