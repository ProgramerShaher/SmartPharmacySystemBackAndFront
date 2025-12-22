using Microsoft.AspNetCore.Mvc;
using SmartPharmacySystem.Application.DTOs.PurchaseReturnDetails;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Application.Wrappers;

namespace SmartPharmacySystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PurchaseReturnDetailsController : ControllerBase
    {
        private readonly IPurchaseReturnDetailService _service;

        public PurchaseReturnDetailsController(IPurchaseReturnDetailService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreatePurchaseReturnDetailDto dto)
        {
            var result = await _service.CreateAsync(dto);
            return Ok(ApiResponse<PurchaseReturnDetailDto>.Succeeded(result, "تم إضافة تفاصيل مرتجع الشراء بنجاح", 201));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdatePurchaseReturnDetailDto dto)
        {
            await _service.UpdateAsync(id, dto);
            return Ok(ApiResponse<object>.Succeeded(null, "تم تحديث تفاصيل مرتجع الشراء بنجاح"));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteAsync(id);
            return Ok(ApiResponse<object>.Succeeded(null, "تم حذف تفاصيل مرتجع الشراء بنجاح"));
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();
            return Ok(ApiResponse<IEnumerable<PurchaseReturnDetailDto>>.Succeeded(result, "تم جلب جميع تفاصيل مرتجعات الشراء بنجاح"));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);
            return Ok(ApiResponse<PurchaseReturnDetailDto>.Succeeded(result, "تم العثور على تفاصيل مرتجع الشراء بنجاح"));
        }
    }
}
