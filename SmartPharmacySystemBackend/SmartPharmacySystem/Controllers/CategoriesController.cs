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

        /// <summary>
        /// Search and paginate categories with optional filters
        /// </summary>
        /// <access>Admin | Pharmacist</access>
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

            await _categoryService.DeleteAsync(id);
            return Ok(ApiResponse<object>.Succeeded(null, "Category deleted successfully"));
        }
    }
}
