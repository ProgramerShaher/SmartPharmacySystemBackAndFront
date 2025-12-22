using Microsoft.AspNetCore.Mvc;
using SmartPharmacySystem.Application.DTOs.StockMovement;
using SmartPharmacySystem.Application.DTOs.Shared;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Application.Wrappers;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Collections.Generic;

namespace SmartPharmacySystem.Controllers
{
    [Route("api/Movements")]
    [ApiController]
    public class StockMovementsController : ControllerBase
    {
        private readonly IStockMovementService _service;
        private readonly ILogger<StockMovementsController> _logger;

        public StockMovementsController(IStockMovementService service, ILogger<StockMovementsController> logger)
        {
            _service = service;
            _logger = logger;
        }

        // -------------------------------------------------------------
        // Search (Audit Trail)
        // -------------------------------------------------------------
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] BaseQueryDto query)
        {
            var result = await _service.SearchAsync(query);
            return Ok(ApiResponse<PagedResult<StockMovementDto>>.Succeeded(result, "تم جلب الحركات المخزنية بنجاح"));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var movement = await _service.GetByIdAsync(id);
            return Ok(ApiResponse<StockMovementDto>.Succeeded(movement, "تم جلب بيانات الحركة بنجاح"));
        }

        // -------------------------------------------------------------
        // Stock Card (Item Ledger)
        // -------------------------------------------------------------
        [HttpGet("stock-card")]
        public async Task<IActionResult> GetStockCard([FromQuery] int medicineId, [FromQuery] int? batchId = null)
        {
            var result = await _service.GetStockCardAsync(medicineId, batchId);
            return Ok(ApiResponse<IEnumerable<StockCardDto>>.Succeeded(result, "تم جلب كرت الصنف بنجاح"));
        }

        // -------------------------------------------------------------
        // Balance
        // -------------------------------------------------------------
        [HttpGet("balance")]
        public async Task<IActionResult> GetBalance([FromQuery] int medicineId, [FromQuery] int? batchId = null)
        {
            var balance = await _service.GetCurrentBalanceAsync(medicineId, batchId);
            return Ok(ApiResponse<int>.Succeeded(balance, "تم جلب رصيد الحالي بنجاح"));
        }

        // -------------------------------------------------------------
        // Manual Movement (Adjustment/Damage ONLY)
        // -------------------------------------------------------------
        [HttpPost("manual")]
        public async Task<IActionResult> CreateManual([FromBody] CreateManualMovementDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Failed("بيانات الحركة غير صحيحة"));

            await _service.CreateManualMovementAsync(dto);
            return Ok(ApiResponse<object>.Succeeded(null, "تم تسجيل الحركة اليدوية بنجاح"));
        }
    }
}
