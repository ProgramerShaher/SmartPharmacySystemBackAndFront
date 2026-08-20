using Microsoft.AspNetCore.Mvc;
using SmartPharmacySystem.Application.DTOs.Customers;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Application.Wrappers;
using SmartPharmacySystem.Authorization;
using System.Security.Claims;

namespace SmartPharmacySystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomerReceiptsController : ControllerBase
    {
        private readonly ICustomerReceiptService _receiptService;

        public CustomerReceiptsController(ICustomerReceiptService receiptService)
        {
            _receiptService = receiptService;
        }

        [RequirePermission("finance.receipts.create")]
        [HttpPost("customer")]
        public async Task<IActionResult> Create([FromBody] CreateCustomerReceiptDto dto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "1");

            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .Select(x => new { Field = x.Key, Errors = x.Value!.Errors.Select(e => e.ErrorMessage).ToList() })
                    .ToList();
                return BadRequest(ApiResponse<string>.Failed(
                    "بيانات السند غير صحيحة: " + string.Join(" | ", errors.Select(e => $"{e.Field}: {string.Join(", ", e.Errors)}"))
                ));
            }

            var result = await _receiptService.CreateAsync(dto, userId);
            return Ok(ApiResponse<CustomerReceiptDto>.Succeeded(result, "تم إصدار سند القبض بنجاح"));
        }

        [RequirePermission("finance.receipts.create")]
        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> Cancel(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "1");
            await _receiptService.CancelAsync(id, userId);
            return Ok(ApiResponse<string>.Succeeded("تم إلغاء سند القبض بنجاح", "تم الإلغاء"));
        }

        [RequirePermission("finance.treasury.view")]
        [HttpGet("customer/{customerId}")]
        public async Task<IActionResult> GetByCustomer(int customerId)
        {
            var result = await _receiptService.GetRecentReceiptsAsync(customerId);
            return Ok(ApiResponse<IEnumerable<CustomerReceiptDto>>.Succeeded(result, "تم جلب السندات الأخيرة"));
        }

        [RequirePermission("finance.treasury.view")]
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
        {
            var result = await _receiptService.GetPagedAsync(search, page, pageSize, fromDate, toDate);
            return Ok(ApiResponse<PagedResponse<CustomerReceiptDto>>.Succeeded(result, "تم جلب سندات القبض بنجاح"));
        }

        [RequirePermission("finance.treasury.view")]
        [HttpGet("stats")]
        public async Task<IActionResult> GetStatistics()
        {
            var result = await _receiptService.GetStatisticsAsync();
            return Ok(ApiResponse<ReceiptStatisticsDto>.Succeeded(result, "تم جلب الإحصائيات"));
        }
    }
}
