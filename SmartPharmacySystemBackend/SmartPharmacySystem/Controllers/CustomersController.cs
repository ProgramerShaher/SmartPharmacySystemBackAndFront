using Microsoft.AspNetCore.Mvc;
using SmartPharmacySystem.Application.DTOs.Customers;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Application.Wrappers;

namespace SmartPharmacySystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomersController : ControllerBase
    {
        private readonly ICustomerService _customerService;

        public CustomersController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _customerService.GetAllPagedAsync(search, page, pageSize);
            return Ok(ApiResponse<PagedResponse<CustomerDto>>.Succeeded(result, "تم جلب العملاء بنجاح"));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _customerService.GetByIdAsync(id);
            return Ok(ApiResponse<CustomerDto>.Succeeded(result, "تم جلب بيانات العميل بنجاح"));
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateCustomerDto dto)
        {
            var result = await _customerService.CreateAsync(dto);
            return Ok(ApiResponse<CustomerDto>.Succeeded(result, "تم إضافة العميل بنجاح"));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateCustomerDto dto)
        {
            if (id != dto.Id) return BadRequest(ApiResponse<string>.Failed("معرف العميل غير متطابق"));
            await _customerService.UpdateAsync(dto);
            return Ok(ApiResponse<string>.Succeeded("تم تحديث بيانات العميل بنجاح", "تم التحديث"));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _customerService.DeleteAsync(id);
            return Ok(ApiResponse<string>.Succeeded("تم حذف العميل بنجاح", "تم الحذف"));
        }

        [HttpGet("top-debtors")]
        public async Task<IActionResult> GetTopDebtors([FromQuery] int count = 5)
        {
            var result = await _customerService.GetTopDebtorsAsync(count);
            return Ok(ApiResponse<IEnumerable<CustomerDto>>.Succeeded(result, "تم جلب قائمة كبار المدينين"));
        }

        [HttpGet("{id}/statement")]
        public async Task<IActionResult> GetStatement(int id)
        {
            var result = await _customerService.GetStatementAsync(id);
            return Ok(ApiResponse<CustomerStatementDto>.Succeeded(result, "تم جلب كشف الحساب بنجاح"));
        }
    }
}
