using Microsoft.AspNetCore.Mvc;
using SmartPharmacySystem.Application.DTOs.PurchaseInvoiceDetails;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Application.Wrappers;

namespace SmartPharmacySystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PurchaseInvoiceDetailsController : ControllerBase
    {
        private readonly IPurchaseInvoiceDetailService _service;
        private readonly ILogger<PurchaseInvoiceDetailsController> _logger;

        public PurchaseInvoiceDetailsController(IPurchaseInvoiceDetailService service, ILogger<PurchaseInvoiceDetailsController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePurchaseInvoiceDetailDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ApiResponse<object>.Failed("بيانات التفاصيل غير صحيحة"));

                var result = await _service.CreateAsync(dto);
                return StatusCode(201, ApiResponse<PurchaseInvoiceDetailDto>.Succeeded(result, "تم إضافة تفاصيل فاتورة الشراء بنجاح", 201));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating purchase invoice detail");
                return StatusCode(500, ApiResponse<object>.Failed("حدث خطأ أثناء إضافة التفاصيل", 500));
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdatePurchaseInvoiceDetailDto dto)
        {
            try
            {
                if (id != dto.Id)
                    return BadRequest(ApiResponse<object>.Failed("رقم التفاصيل غير متطابق"));

                await _service.UpdateAsync(id, dto);
                return Ok(ApiResponse<object>.Succeeded(null, "تم تحديث تفاصيل فاتورة الشراء بنجاح"));
            }
            catch (KeyNotFoundException)
            {
                return NotFound(ApiResponse<object>.Failed($"تفاصيل الفاتورة بالمعرف {id} غير موجودة", 404));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating purchase invoice detail {id}");
                return StatusCode(500, ApiResponse<object>.Failed("حدث خطأ أثناء تحديث التفاصيل", 500));
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(ApiResponse<object>.Failed("رقم التفاصيل غير صحيح"));

                await _service.DeleteAsync(id);
                return Ok(ApiResponse<object>.Succeeded(null, "تم حذف تفاصيل فاتورة الشراء بنجاح"));
            }
             catch (KeyNotFoundException)
            {
                return NotFound(ApiResponse<object>.Failed($"تفاصيل الفاتورة بالمعرف {id} غير موجودة", 404));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting purchase invoice detail {id}");
                return StatusCode(500, ApiResponse<object>.Failed("حدث خطأ أثناء حذف التفاصيل", 500));
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var result = await _service.GetAllAsync();
                return Ok(ApiResponse<IEnumerable<PurchaseInvoiceDetailDto>>.Succeeded(result, "تم جلب جميع تفاصيل فواتير الشراء بنجاح"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching purchase invoice details");
                return StatusCode(500, ApiResponse<object>.Failed("حدث خطأ غير متوقع", 500));
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                 if (id <= 0)
                    return BadRequest(ApiResponse<object>.Failed("رقم التفاصيل غير صحيح"));

                var result = await _service.GetByIdAsync(id);
                if (result == null)
                    return NotFound(ApiResponse<object>.Failed($"تفاصيل الفاتورة بالمعرف {id} غير موجودة", 404));

                return Ok(ApiResponse<PurchaseInvoiceDetailDto>.Succeeded(result, "تم العثور على تفاصيل فاتورة الشراء بنجاح"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching purchase invoice detail {id}");
                return StatusCode(500, ApiResponse<object>.Failed("حدث خطأ غير متوقع", 500));
            }
        }
    }
}
