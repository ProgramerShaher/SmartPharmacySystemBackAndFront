using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartPharmacySystem.Application.DTOs.Shared;
using SmartPharmacySystem.Application.DTOs.Suppliers;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Application.Wrappers;

namespace SmartPharmacySystem.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class SuppliersController : ControllerBase
    {
        private readonly ISupplierService _supplierService;
        private readonly ILogger<SuppliersController> _logger;

        public SuppliersController(ISupplierService supplierService, ILogger<SuppliersController> logger)
        {
            _supplierService = supplierService;
            _logger = logger;
        }

        /// <summary>
        /// Search and paginate suppliers with optional filters
        /// </summary>
        /// <access>Admin | Pharmacist</access>
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] SupplierQueryDto query)
        {
            var result = await _supplierService.SearchAsync(query);

            if (!result.Items.Any())
                return Ok(ApiResponse<PagedResult<SupplierDto>>.Succeeded(result, "No suppliers found matching the search criteria"));

            return Ok(ApiResponse<PagedResult<SupplierDto>>.Succeeded(result, "Suppliers retrieved successfully"));
        }

        /// <summary>
        /// Get supplier by ID
        /// </summary>
        /// <access>Admin | Pharmacist</access>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest(ApiResponse<object>.Failed("Invalid supplier ID provided"));

            var supplier = await _supplierService.GetByIdAsync(id);
            if (supplier == null)
                return NotFound(ApiResponse<object>.Failed($"Supplier with ID {id} not found", 404));

            return Ok(ApiResponse<SupplierDto>.Succeeded(supplier, "Supplier retrieved successfully"));
        }

        /// <summary>
        /// Create a new supplier
        /// </summary>
        /// <access>Admin | Pharmacist</access>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateSupplierDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Failed("Invalid supplier data provided"));

            var created = await _supplierService.CreateAsync(dto);
            return StatusCode(201, ApiResponse<SupplierDto>.Succeeded(created, "Supplier created successfully", 201));
        }

        /// <summary>
        /// Update an existing supplier
        /// </summary>
        /// <access>Admin | Pharmacist</access>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateSupplierDto dto)
        {
            if (id != dto.Id)
                return BadRequest(ApiResponse<object>.Failed("Supplier ID mismatch"));

            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Failed("Invalid supplier data provided"));

            var existing = await _supplierService.GetByIdAsync(id);
            if (existing == null)
                return NotFound(ApiResponse<object>.Failed($"Supplier with ID {id} not found", 404));

            await _supplierService.UpdateAsync(id, dto);
            var updated = await _supplierService.GetByIdAsync(id);
            return Ok(ApiResponse<SupplierDto>.Succeeded(updated, "Supplier updated successfully"));
        }

        /// <summary>
        /// Delete a supplier (soft delete)
        /// </summary>
        /// <access>Admin</access>
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
                return BadRequest(ApiResponse<object>.Failed("Invalid supplier ID provided"));

            var existing = await _supplierService.GetByIdAsync(id);
            if (existing == null)
                return NotFound(ApiResponse<object>.Failed($"Supplier with ID {id} not found", 404));

            await _supplierService.DeleteAsync(id);
            return Ok(ApiResponse<object>.Succeeded(null, "Supplier deleted successfully"));
        }
    }
}
