using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartPharmacySystem.Application.DTOs.Financial;
using SmartPharmacySystem.Application.IServices;
using SmartPharmacySystem.Application.Wrappers;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace SmartPharmacySystem.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ChequesController : ControllerBase
    {
        private readonly IChequeService _chequeService;
        private readonly ILogger<ChequesController> _logger;

        public ChequesController(IChequeService chequeService, ILogger<ChequesController> logger)
        {
            _chequeService = chequeService;
            _logger = logger;
        }

        private int GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
        }

        /// <summary>
        /// جلب الشيكات مع الفلترة والترقيم
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int pageSize = 10, 
            [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null, 
            [FromQuery] string? status = null, [FromQuery] string? type = null)
        {
            var result = await _chequeService.GetPagedAsync(page, pageSize, startDate, endDate, status, type);
            return Ok(ApiResponse<PagedResponse<ChequeDto>>.Succeeded(result, "تم جلب الشيكات بنجاح"));
        }

        /// <summary>
        /// جلب بيانات شيك معين
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var cheque = await _chequeService.GetByIdAsync(id);
                return Ok(ApiResponse<ChequeDto>.Succeeded(cheque, "تم جلب بيانات الشيك بنجاح"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.Failed(ex.Message, 404));
            }
        }

        /// <summary>
        /// تسجيل شيك جديد
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ChequeDto dto)
        {
            try
            {
                var userId = GetUserId();
                var created = await _chequeService.CreateAsync(dto, userId);
                return StatusCode(201, ApiResponse<ChequeDto>.Succeeded(created, "تم تسجيل الشيك بنجاح", 201));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating cheque");
                return BadRequest(ApiResponse<object>.Failed(ex.Message));
            }
        }

        /// <summary>
        /// تحديث حالة الشيك (تحصيل، رفض، إلخ)
        /// </summary>
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromQuery] string status, [FromBody] string? notes = null)
        {
            try
            {
                var userId = GetUserId();
                await _chequeService.UpdateStatusAsync(id, status, userId, notes);
                return Ok(ApiResponse<object>.Succeeded(null, $"تم تحديث حالة الشيك إلى {status} بنجاح"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Failed(ex.Message));
            }
        }

        /// <summary>
        /// جلب الشيكات المستحقة قريباً
        /// </summary>
        [HttpGet("due-soon")]
        public async Task<IActionResult> GetDueSoon([FromQuery] int daysAhead = 7)
        {
            var cheques = await _chequeService.GetDueChequesAsync(daysAhead);
            return Ok(ApiResponse<IEnumerable<ChequeDto>>.Succeeded(cheques, "تم جلب الشيكات المستحقة بنجاح"));
        }
    }
}
