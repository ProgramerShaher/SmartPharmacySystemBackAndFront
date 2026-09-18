using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartPharmacySystem.Application.Wrappers;
using SmartPharmacySystem.Application.DTOs.HeldInvoices;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Application.IServices;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SmartPharmacySystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class HeldInvoicesController : ControllerBase
    {
        private readonly IHeldInvoiceService _heldInvoiceService;
        private readonly ICurrentUserService _currentUserService;

        public HeldInvoicesController(IHeldInvoiceService heldInvoiceService, ICurrentUserService currentUserService)
        {
            _heldInvoiceService = heldInvoiceService;
            _currentUserService = currentUserService;
        }

        private int GetUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(claim, out var id) ? id : 1;
        }

        /// <summary>
        /// تعليق فاتورة جديدة في نقطة البيع
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Hold([FromBody] CreateHeldInvoiceDto dto)
        {
            var userId = GetUserId();
            var branchId = _currentUserService.GetCurrentBranchId();
            var result = await _heldInvoiceService.HoldAsync(dto, userId, branchId);
            return Ok(ApiResponse<HeldInvoiceDto>.Succeeded(result, "تم تعليق الفاتورة بنجاح"));
        }

        /// <summary>
        /// جلب جميع الفواتير المعلقة للفرع الحالي
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var branchId = _currentUserService.GetCurrentBranchId();
            var list = await _heldInvoiceService.GetHeldInvoicesAsync(branchId);
            return Ok(ApiResponse<IEnumerable<HeldInvoiceDto>>.Succeeded(list, "تم جلب الفواتير المعلقة"));
        }

        /// <summary>
        /// جلب تفاصيل فاتورة معلقة بالمعرف
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _heldInvoiceService.GetByIdAsync(id);
            if (item == null)
            {
                return NotFound(ApiResponse<HeldInvoiceDto>.Failed("الفاتورة المعلقة غير موجودة"));
            }
            return Ok(ApiResponse<HeldInvoiceDto>.Succeeded(item, "تم جلب الفاتورة المعلقة"));
        }

        /// <summary>
        /// استئناف أو حذف فاتورة معلقة
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> ResumeOrDelete(int id)
        {
            var success = await _heldInvoiceService.ResumeAndDeleteAsync(id);
            if (!success)
            {
                return NotFound(ApiResponse<bool>.Failed("الفاتورة المعلقة غير موجودة"));
            }
            return Ok(ApiResponse<bool>.Succeeded(true, "تم تحرير الفاتورة المعلقة"));
        }
    }
}
