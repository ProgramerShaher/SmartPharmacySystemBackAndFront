using Microsoft.AspNetCore.Mvc;
using SmartPharmacySystem.Application.DTOs.Medicine;
using SmartPharmacySystem.Application.DTOs.Shared;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Application.Wrappers;

namespace SmartPharmacySystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MedicinesController : ControllerBase
    {
        private readonly IMedicineService _medicineService;
        private readonly ILogger<MedicinesController> _logger;

        public MedicinesController(IMedicineService medicineService, ILogger<MedicinesController> logger)
        {
            _medicineService = medicineService;
            _logger = logger;
        }

        /// <summary>
        /// Search and paginate medicines with optional filters (category, manufacturer, status)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] MedicineQueryDto query)
        {
            var result = await _medicineService.SearchAsync(query);

            if (!result.Items.Any())
                return Ok(ApiResponse<PagedResult<MedicineDto>>.Succeeded(result, "No medicines found matching the search criteria"));

            return Ok(ApiResponse<PagedResult<MedicineDto>>.Succeeded(result, "Medicines retrieved successfully"));
        }

        /// <summary>
        /// Get all medicines without pagination
        /// </summary>
        [HttpGet("all")]
        public async Task<IActionResult> GetAllNoPaging()
        {
            var medicines = await _medicineService.GetAllMedicinesAsync();
            return Ok(ApiResponse<IEnumerable<MedicineDto>>.Succeeded(medicines, "All medicines retrieved successfully"));
        }

        /// <summary>
        /// Get medicine by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest(ApiResponse<object>.Failed("Invalid medicine ID provided"));

            var medicine = await _medicineService.GetMedicineByIdAsync(id);
            if (medicine == null)
                return NotFound(ApiResponse<object>.Failed($"Medicine with ID {id} not found", 404));

            return Ok(ApiResponse<MedicineDto>.Succeeded(medicine, "Medicine retrieved successfully"));
        }

        /// <summary>
        /// Create a new medicine
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateMedicineDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Failed("Invalid medicine data provided"));

            var created = await _medicineService.CreateMedicineAsync(dto);
            return StatusCode(201, ApiResponse<MedicineDto>.Succeeded(created, "Medicine created successfully", 201));
        }

        /// <summary>
        /// Update an existing medicine
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateMedicineDto dto)
        {
            if (id != dto.Id)
                return BadRequest(ApiResponse<object>.Failed("Medicine ID mismatch"));

            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Failed("Invalid medicine data provided"));

            var existing = await _medicineService.GetMedicineByIdAsync(id);
            if (existing == null)
                return NotFound(ApiResponse<object>.Failed($"Medicine with ID {id} not found", 404));

            await _medicineService.UpdateMedicineAsync(id, dto);
            var updated = await _medicineService.GetMedicineByIdAsync(id);
            return Ok(ApiResponse<MedicineDto>.Succeeded(updated, "Medicine updated successfully"));
        }

        /// <summary>
        /// Delete a medicine (soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
                return BadRequest(ApiResponse<object>.Failed("Invalid medicine ID provided"));

            var existing = await _medicineService.GetMedicineByIdAsync(id);
            if (existing == null)
                return NotFound(ApiResponse<object>.Failed($"Medicine with ID {id} not found", 404));

            await _medicineService.DeleteMedicineAsync(id);
            return Ok(ApiResponse<object>.Succeeded(null, "Medicine deleted successfully"));
        }
    }
}
