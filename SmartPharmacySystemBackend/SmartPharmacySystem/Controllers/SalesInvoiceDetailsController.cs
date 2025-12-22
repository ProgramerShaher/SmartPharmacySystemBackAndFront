using Microsoft.AspNetCore.Mvc;
using SmartPharmacySystem.Application.DTOs.SalesInvoiceDetails;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Application.Wrappers;

namespace SmartPharmacySystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SalesInvoiceDetailsController : ControllerBase
    {
        private readonly ISaleInvoiceDetailService _service;

        public SalesInvoiceDetailsController(ISaleInvoiceDetailService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateSaleInvoiceDetailDto dto)
        {

            var result = await _service.CreateAsync(dto);
            return Ok(ApiResponse<SaleInvoiceDetailDto>.Succeeded(result, "تم إضافة تفاصيل فاتورة المبيعات بنجاح", 201));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateSaleInvoiceDetailDto dto)
        {
            await _service.UpdateAsync(id, dto);
            return Ok(ApiResponse<object>.Succeeded(null, "تم تحديث تفاصيل فاتورة المبيعات بنجاح"));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteAsync(id);
            return Ok(ApiResponse<object>.Succeeded(null, "تم حذف تفاصيل فاتورة المبيعات بنجاح"));
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();
            return Ok(ApiResponse<IEnumerable<SaleInvoiceDetailDto>>.Succeeded(result, "تم جلب جميع تفاصيل فواتير المبيعات بنجاح"));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);
            return Ok(ApiResponse<SaleInvoiceDetailDto>.Succeeded(result, "تم العثور على تفاصيل فاتورة المبيعات بنجاح"));
        }
    }
}
