using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartPharmacySystem.Application.DTOs.Inventory;
using SmartPharmacySystem.Application.DTOs.Shared;
using SmartPharmacySystem.Application.IServices;
using SmartPharmacySystem.Application.Wrappers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartPharmacySystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Requires authentication, add specific permission filters if needed
    public class AutomatedAuditsController : ControllerBase
    {
        private readonly IAutomatedAuditService _auditService;

        public AutomatedAuditsController(IAutomatedAuditService auditService)
        {
            _auditService = auditService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] int? warehouseId = null)
        {
            var result = await _auditService.GetAllAsync(page, pageSize, warehouseId);
            return Ok(ApiResponse<PagedResult<AutomatedAuditHeaderDto>>.Succeeded(result, "تم جلب تقارير الجرد الشامل بنجاح"));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _auditService.GetByIdAsync(id);
            return Ok(ApiResponse<AutomatedAuditHeaderDto>.Succeeded(result, "تم جلب تفاصيل الجرد الشامل بنجاح"));
        }

        [HttpPost("generate")]
        public async Task<IActionResult> GenerateAudit([FromBody] GenerateAuditRequestDto request)
        {
            var result = await _auditService.GenerateAuditAsync(request);
            return Ok(ApiResponse<AutomatedAuditHeaderDto>.Succeeded(result, "تم إعداد تقرير الجرد الآلي بنجاح"));
        }

        [HttpGet("{id}/charts")]
        public async Task<IActionResult> GetCharts(int id)
        {
            var result = await _auditService.GetAuditChartsAsync(id);
            return Ok(ApiResponse<AuditChartDataDto>.Succeeded(result, "تم جلب المخططات بنجاح"));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _auditService.DeleteAsync(id);
            return Ok(ApiResponse<string>.Succeeded("تم حذف التقرير", "نجاح"));
        }
    }
}
