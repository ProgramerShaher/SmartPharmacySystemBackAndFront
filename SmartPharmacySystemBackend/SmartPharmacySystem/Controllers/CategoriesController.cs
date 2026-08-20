using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartPharmacySystem.Application.DTOs.Categories;
using SmartPharmacySystem.Application.DTOs.Shared;
using SmartPharmacySystem.Application.Wrappers;

namespace SmartPharmacySystem.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        private readonly ILogger<CategoriesController> _logger;

        public CategoriesController(ICategoryService categoryService, ILogger<CategoriesController> logger)
        {
            _categoryService = categoryService;
            _logger = logger;
        }

        /// <access>Public | Admin | Pharmacist</access>
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] CategoryQueryDto query)
        {
            var result = await _categoryService.SearchAsync(query);

            if (!result.Items.Any())
                return Ok(ApiResponse<PagedResult<CategoryDto>>.Succeeded(result, "No categories found matching the search criteria"));

            return Ok(ApiResponse<PagedResult<CategoryDto>>.Succeeded(result, "Categories retrieved successfully"));
        }

        /// <summary>
        /// Get category by ID
        /// </summary>
        /// <access>Admin | Pharmacist</access>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest(ApiResponse<object>.Failed("Invalid category ID provided"));

            var category = await _categoryService.GetByIdAsync(id);
            if (category == null)
                return NotFound(ApiResponse<object>.Failed($"Category with ID {id} not found", 404));

            return Ok(ApiResponse<CategoryDto>.Succeeded(category, "Category retrieved successfully"));
        }

        /// <summary>
        /// Create a new category
        /// </summary>
        /// <access>Admin | Pharmacist</access>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCategoryDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Failed("Invalid category data provided"));

            var created = await _categoryService.CreateAsync(dto);
            return StatusCode(201, ApiResponse<CategoryDto>.Succeeded(created, "Category created successfully", 201));
        }

        /// <summary>
        /// Update an existing category
        /// </summary>
        /// <access>Admin | Pharmacist</access>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCategoryDto dto)
        {
            if (id != dto.Id)
                return BadRequest(ApiResponse<object>.Failed("Category ID mismatch"));

            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Failed("Invalid category data provided"));

            var existing = await _categoryService.GetByIdAsync(id);
            if (existing == null)
                return NotFound(ApiResponse<object>.Failed($"Category with ID {id} not found", 404));

            await _categoryService.UpdateAsync(id, dto);
            var updated = await _categoryService.GetByIdAsync(id);
            return Ok(ApiResponse<CategoryDto>.Succeeded(updated, "Category updated successfully"));
        }

        /// <summary>
        /// Delete a category (soft delete)
        /// </summary>
        /// <access>Admin</access>
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
                return BadRequest(ApiResponse<object>.Failed("Invalid category ID provided"));

            var existing = await _categoryService.GetByIdAsync(id);
            if (existing == null)
                return NotFound(ApiResponse<object>.Failed($"Category with ID {id} not found", 404));

            try
            {
                await _categoryService.DeleteAsync(id);
                return Ok(ApiResponse<object>.Succeeded(null, "Category deleted successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.Failed(ex.Message));
            }
        }

        /// <summary>
        /// Delete multiple categories
        /// </summary>
        /// <param name="ids">List of category IDs to delete</param>
        /// <returns>Result of the deletion</returns>
        [HttpDelete("bulk")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> DeleteBulk([FromBody] IEnumerable<int> ids)
        {
            if (ids == null || !ids.Any())
                return BadRequest(ApiResponse<object>.Failed("No IDs provided", 400));

            try
            {
                await _categoryService.DeleteBulkAsync(ids);
                return Ok(ApiResponse<object>.Succeeded(null, "Categories deleted successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.Failed(ex.Message));
            }
        }

        /// <summary>
        /// Generate Excel Template for Categories Import
        /// </summary>
        [HttpGet("template")]
        public async Task<IActionResult> GenerateExcelTemplate()
        {
            var content = await _categoryService.GenerateExcelTemplateAsync();
            return File(content, "text/csv", "CategoriesTemplate.csv");
        }

        /// <summary>
        /// Import categories from Excel
        /// </summary>
        [HttpPost("import")]
        public async Task<IActionResult> ImportFromExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(ApiResponse<object>.Failed("لم يتم تحديد أي ملف"));

            var result = await _categoryService.ImportFromExcelAsync(file);

            if (result.FailedCount > 0 && result.SuccessCount == 0)
                return BadRequest(ApiResponse<SmartPharmacySystem.Application.DTOs.Medicine.ImportResultDto>.Succeeded(result, "فشلت عملية الاستيراد كلياً", 400));

            return Ok(ApiResponse<SmartPharmacySystem.Application.DTOs.Medicine.ImportResultDto>.Succeeded(result, "اكتملت عملية الاستيراد"));
        }
    }
}
