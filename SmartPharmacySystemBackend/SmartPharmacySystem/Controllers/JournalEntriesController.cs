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
    public class JournalEntriesController : ControllerBase
    {
        private readonly IJournalEntryService _journalEntryService;
        private readonly ILogger<JournalEntriesController> _logger;

        public JournalEntriesController(IJournalEntryService journalEntryService, ILogger<JournalEntriesController> logger)
        {
            _journalEntryService = journalEntryService;
            _logger = logger;
        }

        private int GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
        }

        /// <summary>
        /// جلب القيود المحاسبية مع الفلترة والترقيم
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int pageSize = 10, 
            [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null, [FromQuery] string? status = null)
        {
            var result = await _journalEntryService.GetPagedAsync(page, pageSize, startDate, endDate, status);
            return Ok(ApiResponse<PagedResponse<JournalEntryDto>>.Succeeded(result, "تم جلب القيود بنجاح"));
        }

        /// <summary>
        /// جلب قيد معين بواسطة المعرف
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var entry = await _journalEntryService.GetByIdAsync(id);
                return Ok(ApiResponse<JournalEntryDto>.Succeeded(entry, "تم جلب بيانات القيد بنجاح"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.Failed(ex.Message, 404));
            }
        }

        /// <summary>
        /// إنشاء قيد محاسبي جديد (مسودة)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] JournalEntryDto dto)
        {
            try
            {
                var userId = GetUserId();
                var created = await _journalEntryService.CreateAsync(dto, userId);
                return StatusCode(201, ApiResponse<JournalEntryDto>.Succeeded(created, "تم إنشاء القيد بنجاح", 201));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.Failed(ex.Message));
            }
        }

        /// <summary>
        /// اعتماد وترحيل القيد المحاسبي
        /// </summary>
        /// <access>Admin | Accountant</access>
        [HttpPost("{id}/approve")]
        public async Task<IActionResult> Approve(int id)
        {
            try
            {
                var userId = GetUserId();
                await _journalEntryService.ApproveAsync(id, userId);
                return Ok(ApiResponse<object>.Succeeded(null, "تم ترحيل القيد بنجاح"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving journal entry {Id}", id);
                return BadRequest(ApiResponse<object>.Failed(ex.Message));
            }
        }

        /// <summary>
        /// إنشاء قيد عكسي لتصحيح خطأ
        /// </summary>
        [HttpPost("{id}/reverse")]
        public async Task<IActionResult> Reverse(int id, [FromBody] string reason)
        {
            try
            {
                var userId = GetUserId();
                await _journalEntryService.ReverseAsync(id, userId, reason);
                return Ok(ApiResponse<object>.Succeeded(null, "تم إنشاء القيد العكسي بنجاح"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Failed(ex.Message));
            }
        }
    }
}
