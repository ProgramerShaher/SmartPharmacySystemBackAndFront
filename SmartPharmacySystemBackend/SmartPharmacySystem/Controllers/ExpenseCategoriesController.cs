using Microsoft.AspNetCore.Mvc;
using SmartPharmacySystem.Application.DTOs.Expense;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Application.Wrappers;

namespace SmartPharmacySystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExpenseCategoriesController : ControllerBase
    {
        private readonly IExpenseService _expenseService;

        public ExpenseCategoriesController(IExpenseService expenseService)
        {
            _expenseService = expenseService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var categories = await _expenseService.GetAllCategoriesAsync();
            return Ok(ApiResponse<IEnumerable<ExpenseCategoryDto>>.Succeeded(categories, "تم جلب الفئات بنجاح"));
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateExpenseCategoryDto dto)
        {
            var category = await _expenseService.CreateCategoryAsync(dto);
            return Ok(ApiResponse<ExpenseCategoryDto>.Succeeded(category, "تم إنشاء الفئة بنجاح"));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateExpenseCategoryDto dto)
        {
            await _expenseService.UpdateCategoryAsync(id, dto);
            return Ok(ApiResponse<string>.Succeeded(id.ToString(), "تم تحديث الفئة بنجاح"));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _expenseService.DeleteCategoryAsync(id);
            return Ok(ApiResponse<string>.Succeeded(id.ToString(), "تم حذف الفئة بنجاح"));
        }
    }
}
