using Microsoft.AspNetCore.Mvc;
using SmartPharmacySystem.Application.DTOs.PurchaseReturns;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Application.Wrappers;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;

namespace SmartPharmacySystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PurchaseReturnsController : ControllerBase
    {
        private readonly IPurchaseReturnService _service;
        private readonly ILogger<PurchaseReturnsController> _logger;

        public PurchaseReturnsController(IPurchaseReturnService service, ILogger<PurchaseReturnsController> logger)
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
            var returns = await _service.GetAllAsync();

            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                returns = returns.Where(r => r.Reason.ToLower().Contains(search));
            }

            if (returns == null || !returns.Any())
                return Ok(ApiResponse<IEnumerable<PurchaseReturnDto>>.Succeeded(new List<PurchaseReturnDto>(), "لا توجد مرتجعات شراء"));

            return Ok(ApiResponse<IEnumerable<PurchaseReturnDto>>.Succeeded(returns, "تم جلب مرتجعات الشراء بنجاح"));
        }

        // -------------------------------------------------------------
        // Get By Id
        // -------------------------------------------------------------
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest(ApiResponse<object>.Failed("رقم المرتجع غير صحيح"));

            var returnItem = await _service.GetByIdAsync(id);
            if (returnItem == null)
                return NotFound(ApiResponse<object>.Failed($"مرتجع الشراء بالمعرف {id} غير موجود", 404));

            return Ok(ApiResponse<PurchaseReturnDto>.Succeeded(returnItem, "تم جلب بيانات المرتجع بنجاح"));
        }

        // -------------------------------------------------------------
        // Create
        // -------------------------------------------------------------
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePurchaseReturnDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Failed("بيانات المرتجع غير صحيحة"));

            // Populate CreatedBy from authenticated user or default (assuming ID 1 is System/Admin)
            int userId = 1;
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int parsedId))
            {
                userId = parsedId;
            }
            dto.CreatedBy = userId;

            var created = await _service.CreateAsync(dto);
            return StatusCode(201, ApiResponse<PurchaseReturnDto>.Succeeded(created, "تم إضافة مرتجع الشراء بنجاح", 201));
        }

        // -------------------------------------------------------------
        // Update
        // -------------------------------------------------------------
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdatePurchaseReturnDto dto)
        {
            if (id != dto.Id)
                return BadRequest(ApiResponse<object>.Failed("رقم المرتجع غير متطابق"));

            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Failed("البيانات المدخلة غير صحيحة"));

            await _service.UpdateAsync(id, dto);

            var updated = await _service.GetByIdAsync(id);
            return Ok(ApiResponse<PurchaseReturnDto>.Succeeded(updated, "تم تحديث بيانات المرتجع بنجاح"));
        }

        // -------------------------------------------------------------
        // Approve
        // -------------------------------------------------------------
        [HttpPost("{id}/approve")]
        public async Task<IActionResult> Approve(int id)
        {
            if (id <= 0)
                return BadRequest(ApiResponse<object>.Failed("رقم المرتجع غير صحيح"));

            await _service.ApproveAsync(id);
            return Ok(ApiResponse<object?>.Succeeded(null, "تم اعتماد المرتجع وتحديث المخزون بنجاح"));
        }

        // -------------------------------------------------------------
        // Cancel
        // -------------------------------------------------------------
        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> Cancel(int id)
        {
            if (id <= 0)
                return BadRequest(ApiResponse<object>.Failed("رقم المرتجع غير صحيح"));

            await _service.CancelAsync(id);
            return Ok(ApiResponse<object?>.Succeeded(null, "تم إلغاء المرتجع وعكس حركات المخزون بنجاح"));
        }

        // -------------------------------------------------------------
        // Delete
        // -------------------------------------------------------------
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
                return BadRequest(ApiResponse<object>.Failed("رقم المرتجع غير صحيح"));

            await _service.DeleteAsync(id);
            return Ok(ApiResponse<object?>.Succeeded(null, "تم حذف المرتجع بنجاح"));
        }
    }
}
