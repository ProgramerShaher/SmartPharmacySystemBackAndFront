using Microsoft.AspNetCore.Authorization;
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
    [Authorize]
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
        public async Task<IActionResult> GetAll([FromQuery] StockMovementQueryDto query)
        {
            var result = await _service.SearchAsync(query);
            return Ok(ApiResponse<PagedResult<StockMovementDto>>.Succeeded(result, "ØªÙ… Ø¬Ù„Ø¨ Ø§Ù„Ø­Ø±ÙƒØ§Øª Ø§Ù„Ù…Ø®Ø²Ù†ÙŠØ© Ø¨Ù†Ø¬Ø§Ø­"));
        }
        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary()
        {
            var result = await _service.GetSummaryAsync();
            return Ok(ApiResponse<StockMovementSummaryDto>.Succeeded(result, "تم جلب ملخص حركات المخزون بنجاح"));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var movement = await _service.GetByIdAsync(id);
            return Ok(ApiResponse<StockMovementDto>.Succeeded(movement, "ØªÙ… Ø¬Ù„Ø¨ Ø¨ÙŠØ§Ù†Ø§Øª Ø§Ù„Ø­Ø±ÙƒØ© Ø¨Ù†Ø¬Ø§Ø­"));
        }

        // -------------------------------------------------------------
        // Stock Card (Item Ledger)
        // -------------------------------------------------------------
        [HttpGet("stock-card")]
        public async Task<IActionResult> GetStockCard([FromQuery] int medicineId, [FromQuery] int? batchId = null)
        {
            var result = await _service.GetStockCardAsync(medicineId, batchId);
            return Ok(ApiResponse<IEnumerable<StockCardDto>>.Succeeded(result, "ØªÙ… Ø¬Ù„Ø¨ ÙƒØ±Øª Ø§Ù„ØµÙ†Ù Ø¨Ù†Ø¬Ø§Ø­"));
        }

        // -------------------------------------------------------------
        // Balance
        // -------------------------------------------------------------
        [HttpGet("balance")]
        public async Task<IActionResult> GetBalance([FromQuery] int medicineId, [FromQuery] int? batchId = null)
        {
            var balance = await _service.GetCurrentBalanceAsync(medicineId, batchId);
            return Ok(ApiResponse<int>.Succeeded(balance, "ØªÙ… Ø¬Ù„Ø¨ Ø±ØµÙŠØ¯ Ø§Ù„Ø­Ø§Ù„ÙŠ Ø¨Ù†Ø¬Ø§Ø­"));
        }

        // -------------------------------------------------------------
        // Manual Movement (Adjustment/Damage ONLY)
        // -------------------------------------------------------------
        /// <summary>
        /// Create manual stock movement (Adjustment/Damage)
        /// </summary>
        /// <access>Admin</access>
        [Authorize(Roles = "Admin")]
        [HttpPost("manual")]
        public async Task<IActionResult> CreateManual([FromBody] CreateManualMovementDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Failed("Ø¨ÙŠØ§Ù†Ø§Øª Ø§Ù„Ø­Ø±ÙƒØ© ØºÙŠØ± ØµØ­ÙŠØ­Ø©"));

            await _service.CreateManualMovementAsync(dto);
            return Ok(ApiResponse<object>.Succeeded(null, "ØªÙ… ØªØ³Ø¬ÙŠÙ„ Ø§Ù„Ø­Ø±ÙƒØ© Ø§Ù„ÙŠØ¯ÙˆÙŠØ© Ø¨Ù†Ø¬Ø§Ø­"));
        }
    }
}


