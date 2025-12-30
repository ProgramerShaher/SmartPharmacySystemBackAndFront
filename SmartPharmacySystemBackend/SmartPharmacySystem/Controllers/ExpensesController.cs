using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartPharmacySystem.Application.DTOs.Expense;
using SmartPharmacySystem.Application.DTOs.Shared;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Application.Wrappers;

namespace SmartPharmacySystem.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class ExpensesController : ControllerBase
    {
        private readonly IExpenseService _expenseService;
        private readonly ILogger<ExpensesController> _logger;

        public ExpensesController(IExpenseService expenseService, ILogger<ExpensesController> logger)
        {
            _expenseService = expenseService;
            _logger = logger;
        }

        /// <summary>
        /// Search and paginate expenses with optional filters (date range, expense type)
        /// </summary>
        /// <access>Admin</access>
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] ExpenseQueryDto query)
        {
            var result = await _expenseService.SearchAsync(query);

            if (!result.Items.Any())
                return Ok(ApiResponse<PagedResult<ExpenseDto>>.Succeeded(result, "No expenses found matching the search criteria"));

            return Ok(ApiResponse<PagedResult<ExpenseDto>>.Succeeded(result, "Expenses retrieved successfully"));
        }

        /// <summary>
        /// Get expense by ID
        /// </summary>
        /// <access>Admin</access>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest(ApiResponse<object>.Failed("Invalid expense ID provided"));

            var expense = await _expenseService.GetExpenseByIdAsync(id);
            if (expense == null)
                return NotFound(ApiResponse<object>.Failed($"Expense with ID {id} not found", 404));

            return Ok(ApiResponse<ExpenseDto>.Succeeded(expense, "Expense retrieved successfully"));
        }

        /// <summary>
        /// Create a new expense
        /// </summary>
        /// <access>Admin</access>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateExpenseDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Failed("Invalid expense data provided"));

            // Populate CreatedBy from authenticated user or default (assuming ID 1 is System/Admin)
            // If we can't parse the User ID, we default to 1 (or another valid default ID in your DB)
            int userId = 1;
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int parsedId))
            {
                userId = parsedId;
            }

            dto.CreatedBy = userId;

            var created = await _expenseService.CreateExpenseAsync(dto);
            return StatusCode(201, ApiResponse<ExpenseDto>.Succeeded(created, "Expense created successfully", 201));
        }

        /// <summary>
        /// Update an existing expense
        /// </summary>
        /// <access>Admin</access>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateExpenseDto dto)
        {
            if (id != dto.Id)
                return BadRequest(ApiResponse<object>.Failed("Expense ID mismatch"));

            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Failed("Invalid expense data provided"));

            var existing = await _expenseService.GetExpenseByIdAsync(id);
            if (existing == null)
                return NotFound(ApiResponse<object>.Failed($"Expense with ID {id} not found", 404));

            await _expenseService.UpdateExpenseAsync(id, dto);
            var updated = await _expenseService.GetExpenseByIdAsync(id);
            return Ok(ApiResponse<ExpenseDto>.Succeeded(updated, "Expense updated successfully"));
        }

        /// <summary>
        /// Delete an expense (soft delete)
        /// </summary>
        /// <access>Admin</access>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
                return BadRequest(ApiResponse<object>.Failed("Invalid expense ID provided"));

            var existing = await _expenseService.GetExpenseByIdAsync(id);
            if (existing == null)
                return NotFound(ApiResponse<object>.Failed($"Expense with ID {id} not found", 404));

            await _expenseService.DeleteExpenseAsync(id);
            return Ok(ApiResponse<object>.Succeeded(null, "Expense deleted successfully"));
        }
    }
}
