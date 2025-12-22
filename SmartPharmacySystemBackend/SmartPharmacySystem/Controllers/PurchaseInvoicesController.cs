using Microsoft.AspNetCore.Mvc;
using SmartPharmacySystem.Application.DTOs.CreatePurchaseInvoice;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Application.Wrappers;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;

namespace SmartPharmacySystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PurchaseInvoicesController : ControllerBase
    {
        private readonly IPurchaseInvoiceService _service;
        private readonly ILogger<PurchaseInvoicesController> _logger;

        public PurchaseInvoicesController(IPurchaseInvoiceService service, ILogger<PurchaseInvoicesController> logger)
        {
            _service = service;
            _logger = logger;
        }

        // -------------------------------------------------------------
        // Get All / Search
        // -------------------------------------------------------------
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? search = null)
        {
            var invoices = await _service.GetAllAsync();

            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                invoices = invoices.Where(i => (i.SupplierName != null && i.SupplierName.ToLower().Contains(search)) ||
                                               (i.SupplierInvoiceNumber != null && i.SupplierInvoiceNumber.Contains(search)) ||
                                               (i.Notes != null && i.Notes.ToLower().Contains(search)));
            }

            if (invoices == null || !invoices.Any())
                return Ok(ApiResponse<IEnumerable<PurchaseInvoiceDto>>.Succeeded(new List<PurchaseInvoiceDto>(), "لا توجد فواتير شراء"));

            return Ok(ApiResponse<IEnumerable<PurchaseInvoiceDto>>.Succeeded(invoices, "تم جلب فواتير الشراء بنجاح"));
        }

        // -------------------------------------------------------------
        // Get By Id
        // -------------------------------------------------------------
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest(ApiResponse<object>.Failed("رقم الفاتورة غير صحيح"));

            var invoice = await _service.GetByIdAsync(id);
            if (invoice == null)
                return NotFound(ApiResponse<object>.Failed($"فاتورة الشراء بالمعرف {id} غير موجودة", 404));

            return Ok(ApiResponse<PurchaseInvoiceDto>.Succeeded(invoice, "تم جلب بيانات الفاتورة بنجاح"));
        }

        // -------------------------------------------------------------
        // Create
        // -------------------------------------------------------------
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePurchaseInvoiceDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Failed("بيانات الفاتورة غير صحيحة"));

            // Populate CreatedBy from authenticated user or default (assuming ID 1 is System/Admin)
            int userId = 1;
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int parsedId))
            {
                userId = parsedId;
            }
            dto.CreatedBy = userId;

            var created = await _service.CreateAsync(dto);
            return StatusCode(201, ApiResponse<PurchaseInvoiceDto>.Succeeded(created, "تم إضافة فاتورة الشراء بنجاح", 201));
        }

        // -------------------------------------------------------------
        // Update
        // -------------------------------------------------------------
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdatePurchaseInvoiceDto dto)
        {
            if (id != dto.Id)
                return BadRequest(ApiResponse<object>.Failed("رقم الفاتورة غير متطابق"));

            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Failed("البيانات المدخلة غير صحيحة"));

            await _service.UpdateAsync(id, dto);

            var updated = await _service.GetByIdAsync(id);
            return Ok(ApiResponse<PurchaseInvoiceDto>.Succeeded(updated, "تم تحديث بيانات الفاتورة بنجاح"));
        }

        // -------------------------------------------------------------
        // Approve
        // -------------------------------------------------------------
        [HttpPost("{id}/approve")]
        public async Task<IActionResult> Approve(int id)
        {
            if (id <= 0)
                return BadRequest(ApiResponse<object>.Failed("رقم الفاتورة غير صحيح"));

            await _service.ApproveAsync(id);
            return Ok(ApiResponse<object?>.Succeeded(null, "تم اعتماد الفاتورة وتحديث المخزون بنجاح"));
        }

        // -------------------------------------------------------------
        // Cancel
        // -------------------------------------------------------------
        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> Cancel(int id)
        {
            if (id <= 0)
                return BadRequest(ApiResponse<object>.Failed("رقم الفاتورة غير صحيح"));

            await _service.CancelAsync(id);
            return Ok(ApiResponse<object?>.Succeeded(null, "تم إلغاء الفاتورة وعكس حركات المخزون بنجاح"));
        }

        // -------------------------------------------------------------
        // Delete
        // -------------------------------------------------------------
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
                return BadRequest(ApiResponse<object>.Failed("رقم الفاتورة غير صحيح"));

            await _service.DeleteAsync(id);
            return Ok(ApiResponse<object?>.Succeeded(null, "تم حذف الفاتورة بنجاح"));
        }
    }
}
