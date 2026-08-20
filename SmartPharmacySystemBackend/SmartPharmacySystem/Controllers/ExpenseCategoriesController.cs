using Microsoft.AspNetCore.Mvc;
using SmartPharmacySystem.Application.DTOs.Expense;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Application.Wrappers;

using Microsoft.AspNetCore.Authorization;
using SmartPharmacySystem.Authorization;

namespace SmartPharmacySystem.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ExpenseCategoriesController : ControllerBase
    {
        private readonly IExpenseService _expenseService;

        public ExpenseCategoriesController(IExpenseService expenseService)
        {
            _expenseService = expenseService;
        }

        [RequirePermission("finance.expenses.view")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var categories = await _expenseService.GetAllCategoriesAsync();
            return Ok(ApiResponse<IEnumerable<ExpenseCategoryDto>>.Succeeded(categories, "تم جلب الفئات بنجاح"));
        }

        [RequirePermission("finance.expenses.create")]
        [HttpPost]
        public async Task<IActionResult> Create(CreateExpenseCategoryDto dto)
        {
            var category = await _expenseService.CreateCategoryAsync(dto);
            return Ok(ApiResponse<ExpenseCategoryDto>.Succeeded(category, "تم إنشاء الفئة بنجاح"));
        }

        [RequirePermission("finance.expenses.create")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateExpenseCategoryDto dto)
        {
            await _expenseService.UpdateCategoryAsync(id, dto);
            return Ok(ApiResponse<string>.Succeeded(id.ToString(), "تم تحديث الفئة بنجاح"));
        }

        [RequirePermission("finance.expenses.create")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _expenseService.DeleteCategoryAsync(id);
            return Ok(ApiResponse<string>.Succeeded(id.ToString(), "تم حذف الفئة بنجاح"));
        }
    }
}
