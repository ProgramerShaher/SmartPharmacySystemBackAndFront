using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartPharmacySystem.Application.DTOs.Notifications;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Application.Wrappers;
using SmartPharmacySystem.Core.Interfaces;

namespace SmartPharmacySystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly IAlertService _alertService;
        private readonly INotificationService _notificationService;
        private readonly AutoMapper.IMapper _mapper;

        public NotificationsController(IAlertService alertService, INotificationService notificationService, AutoMapper.IMapper mapper)
        {
            _alertService = alertService;
            _notificationService = notificationService;
            _mapper = mapper;
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

        [HttpGet("unread")]
        public async Task<IActionResult> GetUnread([FromQuery] int userId, [FromQuery] int? branchId)
        {
            var notifications = await _notificationService.GetUnreadNotificationsAsync(userId, branchId);
            var dtos = _mapper.Map<IEnumerable<NotificationDto>>(notifications);
            return Ok(ApiResponse<IEnumerable<NotificationDto>>.Succeeded(dtos, "تم جلب الإشعارات بنجاح"));
        }

        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id, [FromQuery] int userId)
        {
            await _notificationService.MarkAsReadAsync(id, userId);
            return Ok(ApiResponse<bool>.Succeeded(true, "تم تحديد الإشعار كمقروء"));
        }

        [HttpPut("read-all")]
        public async Task<IActionResult> MarkAllAsRead([FromQuery] int userId, [FromQuery] int? branchId)
        {
            await _notificationService.MarkAllAsReadAsync(userId, branchId);
            return Ok(ApiResponse<bool>.Succeeded(true, "تم تحديد كافة الإشعارات كمقروءة"));
        }
    }
}
