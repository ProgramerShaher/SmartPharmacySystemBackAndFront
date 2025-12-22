using Microsoft.AspNetCore.Mvc;
using SmartPharmacySystem.Application.DTOs.SalesReturnDetails;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Application.Wrappers;

namespace SmartPharmacySystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SalesReturnDetailsController : ControllerBase
    {
        private readonly ISalesReturnDetailService _service;

        public SalesReturnDetailsController(ISalesReturnDetailService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateSalesReturnDetailDto dto)
        {
            var result = await _service.CreateAsync(dto);
            return Ok(ApiResponse<SalesReturnDetailDto>.Succeeded(null, "تم إضافة تفاصيل مرتجع المبيعات بنجاح", 201));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateSalesReturnDetailDto dto)
        {
            await _service.UpdateAsync(id, dto);
            return Ok(ApiResponse<object>.Succeeded(null, "تم تحديث تفاصيل مرتجع المبيعات بنجاح"));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteAsync(id);
            return Ok(ApiResponse<object>.Succeeded(null, "تم حذف تفاصيل مرتجع المبيعات بنجاح"));
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();
            return Ok(ApiResponse<IEnumerable<SalesReturnDetailDto>>.Succeeded(null, "تم جلب جميع تفاصيل مرتجعات المبيعات بنجاح"));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);
            return Ok(ApiResponse<SalesReturnDetailDto>.Succeeded(null, "تم العثور على تفاصيل مرتجع المبيعات بنجاح"));
        }
    }
}