using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SmartPharmacySystem.Application.DTOs.SalesInvoices;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Application.Wrappers;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using Microsoft.OpenApi.Extensions;

namespace SmartPharmacySystem.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class SalesInvoicesController : ControllerBase
    {
        private readonly ISaleInvoiceService _service;
        private readonly ILogger<SalesInvoicesController> _logger;

        public SalesInvoicesController(ISaleInvoiceService service, ILogger<SalesInvoicesController> logger)
        {
            _service = service;
            _logger = logger;
        }

        // -------------------------------------------------------------
        // Get All / Search
        // -------------------------------------------------------------
        /// <summary>
        /// Get all sales invoices
        /// </summary>
        /// <access>Admin | Pharmacist</access>
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? search = null)
        {
            var invoices = await _service.GetAllAsync();

            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                invoices = invoices.Where(i => i.CustomerName.ToLower().Contains(search) ||
                                               i.PaymentMethod.GetDisplayName().Contains(search));
            }

            if (invoices == null || !invoices.Any())
                return Ok(ApiResponse<IEnumerable<SaleInvoiceDto>>.Succeeded(new List<SaleInvoiceDto>(), "لا توجد فواتير مبيعات"));

            return Ok(ApiResponse<IEnumerable<SaleInvoiceDto>>.Succeeded(invoices, "تم جلب فواتير المبيعات بنجاح"));
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
                return NotFound(ApiResponse<object>.Failed($"فاتورة المبيعات بالمعرف {id} غير موجودة", 404));

            return Ok(ApiResponse<SaleInvoiceDto>.Succeeded(invoice, "تم جلب بيانات الفاتورة بنجاح"));
        }

        // -------------------------------------------------------------
        // Create
        // -------------------------------------------------------------
        [HttpPost]
        public async Task<Results<Created<ApiResponse<SaleInvoiceDto>>, BadRequest<ApiResponse<object>>>> Create([FromBody] CreateSaleInvoiceDto dto)
        {
            if (!ModelState.IsValid)
                return TypedResults.BadRequest(ApiResponse<object>.Failed("بيانات الفاتورة غير صحيحة"));

            // Populate CreatedBy from authenticated user or default (assuming ID 1 is System/Admin)
            int userId = 1;
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int parsedId))
            {
                userId = parsedId;
            }
            var created = await _service.CreateAsync(dto, userId);
            return TypedResults.Created($"/api/SalesInvoices/{created.Id}",
                ApiResponse<SaleInvoiceDto>.Succeeded(created, "تم إضافة فاتورة المبيعات بنجاح", 201));
        }

        // -------------------------------------------------------------
        // Update
        // -------------------------------------------------------------
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateSaleInvoiceDto dto)
        {
            if (id != dto.Id)
                return BadRequest(ApiResponse<object>.Failed("رقم الفاتورة غير متطابق"));

            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Failed("البيانات المدخلة غير صحيحة"));

            await _service.UpdateAsync(id, dto);

            var updated = await _service.GetByIdAsync(id);
            return Ok(ApiResponse<SaleInvoiceDto>.Succeeded(updated, "تم تحديث بيانات الفاتورة بنجاح"));
        }

        // -------------------------------------------------------------
        // Approve
        // -------------------------------------------------------------
        /// <summary>
        /// Approve sales invoice
        /// </summary>
        /// <access>Admin</access>
        [Authorize(Roles = "Admin")]
        [HttpPost("{id}/approve")]
        public async Task<Results<Ok<ApiResponse<object>>, BadRequest<ApiResponse<object>>>> Approve(int id)
        {
            if (id <= 0)
                return TypedResults.BadRequest(ApiResponse<object>.Failed("رقم الفاتورة غير صحيح"));

            try
            {
                int userId = 1;
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int parsedId))
                {
                    userId = parsedId;
                }

                await _service.ApproveAsync(id, userId);
                return TypedResults.Ok(ApiResponse<object>.Succeeded(new { }, "تم اعتماد الفاتورة وتحديث المخزون بنجاح"));
            }
            catch (Exception ex)
            {
                return TypedResults.BadRequest(ApiResponse<object>.Failed(ex.Message));
            }
        }

        /// <summary>
        /// Unapprove sales invoice
        /// </summary>
        /// <access>Admin</access>
        [Authorize(Roles = "Admin")]
        [HttpPost("{id}/unapprove")]
        public async Task<IActionResult> Unapprove(int id)
        {
            if (id <= 0)
                return BadRequest(ApiResponse<object>.Failed("رقم الفاتورة غير صحيح"));

            await _service.UnapproveSalesInvoiceAsync(id);
            return Ok(ApiResponse<object?>.Succeeded(null, "تم إلغاء اعتماد الفاتورة وعكس حركات المخزون والمالية بنجاح"));
        }

        // -------------------------------------------------------------
        // Cancel
        // -------------------------------------------------------------
        /// <summary>
        /// Cancel sales invoice
        /// </summary>
        /// <access>Admin</access>
        [Authorize(Roles = "Admin")]
        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> Cancel(int id)
        {
            if (id <= 0)
                return BadRequest(ApiResponse<object>.Failed("رقم الفاتورة غير صحيح"));

            int userId = 1;
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int parsedId))
            {
                userId = parsedId;
            }

            await _service.CancelAsync(id, userId);
            return Ok(ApiResponse<object?>.Succeeded(null, "تم إلغاء الفاتورة وعكس حركات المخزون بنجاح"));
        }

        // -------------------------------------------------------------
        // Delete
        // -------------------------------------------------------------
        /// <summary>
        /// Delete sales invoice
        /// </summary>
        /// <access>Admin</access>
        [Authorize(Roles = "Admin")]
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
