using Microsoft.AspNetCore.Mvc;
using SmartPharmacySystem.Application.DTOs.Alerts;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Application.Wrappers;
using Microsoft.Extensions.Logging;
using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AlertsController : ControllerBase
    {
        private readonly IAlertService _alertService;
        private readonly ILogger<AlertsController> _logger;

        public AlertsController(IAlertService alertService, ILogger<AlertsController> logger)
        {
            _alertService = alertService;
            _logger = logger;
        }

        // -------------------------------------------------------------
        // Get All
        // -------------------------------------------------------------
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var alerts = await _alertService.GetAllAsync();
            if (alerts == null || !alerts.Any())
                return Ok(ApiResponse<IEnumerable<AlertDto>>.Succeeded(new List<AlertDto>(), "لا توجد تنبيهات"));

            return Ok(ApiResponse<IEnumerable<AlertDto>>.Succeeded(alerts, "تم جلب التنبيهات بنجاح"));
        }

        // -------------------------------------------------------------
        // Get By Id
        // -------------------------------------------------------------
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest(ApiResponse<object>.Failed("رقم التنبيه غير صحيح"));

            var alert = await _alertService.GetByIdAsync(id);
            if (alert == null)
                return NotFound(ApiResponse<object>.Failed($"التنبيه بالمعرف {id} غير موجود", 404));

            return Ok(ApiResponse<AlertDto>.Succeeded(alert, "تم جلب التنبيه بنجاح"));
        }

        // -------------------------------------------------------------
        // Get By Status
        // -------------------------------------------------------------
        [HttpGet("status/{status}")]
        public async Task<IActionResult> GetByStatus(AlertStatus status)
        {
            var alerts = await _alertService.GetByStatusAsync(status);
            return Ok(ApiResponse<IEnumerable<AlertDto>>.Succeeded(alerts, "تم جلب التنبيهات حسب الحالة"));
        }

        // -------------------------------------------------------------
        // Get By Batch
        // -------------------------------------------------------------
        [HttpGet("batch/{batchId}")]
        public async Task<IActionResult> GetByBatch(int batchId)
        {
            var alerts = await _alertService.GetByBatchIdAsync(batchId);
            return Ok(ApiResponse<IEnumerable<AlertDto>>.Succeeded(alerts, "تم جلب التنبيهات للدفعة المحددة"));
        }

        // -------------------------------------------------------------
        // Mark As Read
        // -------------------------------------------------------------
        [HttpPost("{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            await _alertService.MarkAsReadAsync(id);
            return Ok(ApiResponse<object>.Succeeded(null, "تم تحديد التنبيه كمقروء"));
        }

        // -------------------------------------------------------------
        // Generate Expiry Alerts
        // -------------------------------------------------------------
        [HttpPost("generate-expiry")]
        public async Task<IActionResult> GenerateExpiry()
        {
            await _alertService.GenerateExpiryAlertsAsync();
            return Ok(ApiResponse<object>.Succeeded(null, "تم توليد تنبيهات الصلاحية بنجاح"));
        }

        // -------------------------------------------------------------
        // Create
        // -------------------------------------------------------------
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateAlertDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Failed("بيانات التنبيه غير صحيحة"));

            var created = await _alertService.CreateAsync(dto);
            return StatusCode(201, ApiResponse<AlertDto>.Succeeded(created, "تم إنشاء التنبيه بنجاح", 201));
        }

        // -------------------------------------------------------------
        // Update
        // -------------------------------------------------------------
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateAlertDto dto)
        {
            if (id != dto.Id)
                return BadRequest(ApiResponse<object>.Failed("رقم التنبيه غير متطابق"));

            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Failed("البيانات المدخلة غير صحيحة"));

            await _alertService.UpdateAsync(id, dto);
            var updated = await _alertService.GetByIdAsync(id);
            return Ok(ApiResponse<AlertDto>.Succeeded(updated, "تم تحديث التنبيه بنجاح"));
        }

        // -------------------------------------------------------------
        // Delete
        // -------------------------------------------------------------
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _alertService.DeleteAsync(id);
            if (!success)
                 return NotFound(ApiResponse<object>.Failed($"التنبيه {id} غير موجود", 404));

            return Ok(ApiResponse<object>.Succeeded(null, "تم حذف التنبيه بنجاح"));
        }
    }
}
