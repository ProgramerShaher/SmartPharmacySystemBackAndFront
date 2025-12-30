using Microsoft.AspNetCore.Mvc;
using SmartPharmacySystem.Application.DTOs.Customers;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Application.Wrappers;
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

        [HttpPost]
        public async Task<IActionResult> Create(CreateCustomerReceiptDto dto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "1");
            var result = await _receiptService.CreateAsync(dto, userId);
            return Ok(ApiResponse<CustomerReceiptDto>.Succeeded(result, "تم إصدار سند القبض بنجاح"));
        }

        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> Cancel(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "1");
            await _receiptService.CancelAsync(id, userId);
            return Ok(ApiResponse<string>.Succeeded("تم إلغاء سند القبض بنجاح", "تم الإلغاء"));
        }

        [HttpGet("customer/{customerId}")]
        public async Task<IActionResult> GetByCustomer(int customerId)
        {
            var result = await _receiptService.GetRecentReceiptsAsync(customerId);
            return Ok(ApiResponse<IEnumerable<CustomerReceiptDto>>.Succeeded(result, "تم جلب السندات الأخيرة"));
        }
    }
}
