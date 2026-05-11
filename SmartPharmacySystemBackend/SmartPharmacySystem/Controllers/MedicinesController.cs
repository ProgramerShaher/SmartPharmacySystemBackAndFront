using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartPharmacySystem.Application.DTOs.Medicine;
using SmartPharmacySystem.Application.DTOs.Shared;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Application.Wrappers;

namespace SmartPharmacySystem.Controllers
{
    [Authorize]
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

        /// <access>Public | Admin | Pharmacist</access>
        [AllowAnonymous]
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
        /// <access>Admin | Pharmacist</access>
        [HttpGet("all")]
        public async Task<IActionResult> GetAllNoPaging()
        {
            var medicines = await _medicineService.GetAllMedicinesAsync();
            return Ok(ApiResponse<IEnumerable<MedicineDto>>.Succeeded(medicines, "All medicines retrieved successfully"));
        }

        /// <summary>
        /// Get medicine by ID
        /// </summary>
        /// <access>Admin | Pharmacist</access>
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
        /// <access>Admin | Pharmacist</access>
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
        /// <access>Admin | Pharmacist</access>
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
        /// Delete a medicine (soft delete or set Inactive based on movements)
        /// </summary>
        /// <access>Admin</access>
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(ApiResponse<object>.Failed("Invalid medicine ID provided"));

                await _medicineService.DeleteMedicineAsync(id);
                return Ok(ApiResponse<object>.Succeeded(null, "Medicine deleted successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.Failed(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting medicine {Id}", id);
                return StatusCode(500, ApiResponse<object>.Failed("An error occurred while deleting the medicine"));
            }
        }

        /// <summary>
        /// Get full medicine details with batches and stock metrics
        /// </summary>
        [HttpGet("{id}/details")]
        public async Task<IActionResult> GetDetails(int id)
        {
            if (id <= 0)
                return BadRequest(ApiResponse<object>.Failed("Invalid medicine ID provided"));

            var details = await _medicineService.GetMedicineDetailsAsync(id);
            if (details == null)
                return NotFound(ApiResponse<object>.Failed($"Medicine with ID {id} not found", 404));

            return Ok(ApiResponse<MedicineDetailsDto>.Succeeded(details, "Medicine details retrieved successfully"));
        }

        /// <summary>
        /// Get batches suggest for a medicine by FEFO (First Expired First Out)
        /// </summary>
        [HttpGet("{id}/fefo-batches")]
        public async Task<IActionResult> GetFEFOBatches(int id)
        {
            var batches = await _medicineService.GetBatchesByFEFOAsync(id);
            return Ok(ApiResponse<IEnumerable<SmartPharmacySystem.Application.DTOs.MedicineBatch.MedicineBatchResponseDto>>.Succeeded(batches, "FEFO batches retrieved successfully"));
        }

        /// <summary>
        /// Get report of medicines that reached reorder level
        /// </summary>
        [HttpGet("reorder-report")]
        public async Task<IActionResult> GetReorderReport()
        {
            var report = await _medicineService.GetReorderReportAsync();
            return Ok(ApiResponse<IEnumerable<MedicineDto>>.Succeeded(report, "Reorder report generated successfully"));
        }
    }
}
