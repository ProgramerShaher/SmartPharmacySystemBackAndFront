using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartPharmacySystem.Application.DTOs.SupplierPayments;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Application.Wrappers;
using SmartPharmacySystem.Core.Interfaces;

namespace SmartPharmacySystem.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class SupplierPaymentsController : ControllerBase
    {
        private readonly ISupplierPaymentService _paymentService;
        private readonly ICurrentUserService _currentUserService;

        public SupplierPaymentsController(ISupplierPaymentService paymentService, ICurrentUserService currentUserService)
        {
            _paymentService = paymentService;
            _currentUserService = currentUserService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateSupplierPaymentDto dto)
        {
            var userId = _currentUserService.UserId ?? 1;
            var result = await _paymentService.CreateAsync(dto, userId);
            return Ok(ApiResponse<SupplierPaymentDto>.Succeeded(result, "تم تسجيل سند الصرف بنجاح."));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Cancel(int id)
        {
            var userId = _currentUserService.UserId ?? 1;
            await _paymentService.CancelAsync(id, userId);
            return Ok(ApiResponse<object>.Succeeded(null!, "تم إلغاء سند الصرف بنجاح."));
        }

        [HttpGet("supplier/{supplierId}/statement")]
        public async Task<IActionResult> GetStatement(int supplierId)
        {
            var result = await _paymentService.GetStatementAsync(supplierId);
            return Ok(ApiResponse<SupplierStatementDto>.Succeeded(result, "تم جلب كشف الحساب بنجاح."));
        }

        [HttpGet("supplier/{supplierId}/recent-payments")]
        public async Task<IActionResult> GetRecentPayments(int supplierId)
        {
            var result = await _paymentService.GetRecentPaymentsAsync(supplierId);
            return Ok(ApiResponse<IEnumerable<SupplierPaymentDto>>.Succeeded(result, "تم جلب السندات الأخيرة بنجاح."));
        }
    }
}
