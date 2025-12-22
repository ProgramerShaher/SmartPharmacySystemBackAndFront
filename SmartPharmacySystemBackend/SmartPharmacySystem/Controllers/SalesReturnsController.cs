using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SmartPharmacySystem.Application.DTOs.SalesReturns;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Application.Wrappers;

namespace SmartPharmacySystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SalesReturnsController : ControllerBase
    {
        private readonly ISalesReturnService _service;
        private readonly ILogger<SalesReturnsController> _logger;

        public SalesReturnsController(ISalesReturnService service, ILogger<SalesReturnsController> logger)
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
                return Ok(ApiResponse<IEnumerable<SalesReturnDto>>.Succeeded(new List<SalesReturnDto>(), "لا توجد مرتجعات مبيعات"));

            return Ok(ApiResponse<IEnumerable<SalesReturnDto>>.Succeeded(returns, "تم جلب مرتجعات المبيعات بنجاح"));
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
                return NotFound(ApiResponse<object>.Failed($"مرتجع المبيعات بالمعرف {id} غير موجود", 404));

            return Ok(ApiResponse<SalesReturnDto>.Succeeded(returnItem, "تم جلب بيانات المرتجع بنجاح"));
        }

        // -------------------------------------------------------------
        // Create
        // -------------------------------------------------------------
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateSalesReturnDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Failed("بيانات المرتجع غير صحيحة"));

            var created = await _service.CreateAsync(dto);
            return StatusCode(201, ApiResponse<SalesReturnDto>.Succeeded(created, "تم إضافة مرتجع المبيعات بنجاح", 201));
        }

        // -------------------------------------------------------------
        // Update
        // -------------------------------------------------------------
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateSalesReturnDto dto)
        {
            if (id != dto.Id)
                return BadRequest(ApiResponse<object>.Failed("رقم المرتجع غير متطابق"));

            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Failed("البيانات المدخلة غير صحيحة"));

            await _service.UpdateAsync(id, dto);

            var updated = await _service.GetByIdAsync(id);
            return Ok(ApiResponse<SalesReturnDto>.Succeeded(updated, "تم تحديث بيانات المرتجع بنجاح"));
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
