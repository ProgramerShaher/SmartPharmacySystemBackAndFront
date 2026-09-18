using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartPharmacySystem.Application.DTOs.Quotations;
using SmartPharmacySystem.Application.DTOs.SalesInvoices;
using SmartPharmacySystem.Application.DTOs.Shared;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Application.Wrappers;
using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Controllers;

/// <summary>
/// متحكم إدارة عروض الأسعار وفواتير العروض المبدئية وتحويلها إلى فواتير مبيعات
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class QuotationsController : ControllerBase
{
    private readonly IQuotationService _quotationService;
    private readonly ICurrentUserService _currentUserService;

    public QuotationsController(
        IQuotationService quotationService,
        ICurrentUserService currentUserService)
    {
        _quotationService = quotationService;
        _currentUserService = currentUserService;
    }

    private int GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(claim, out var id) ? id : (_currentUserService.UserId ?? 1);
    }

    /// <summary>
    /// جلب قائمة عروض الأسعار مع التصفية والصفحات
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] QuotationFilterDto filter)
    {
        // إذا لم يتم تحديد فرع وكان المستخدم ينتمي لفرع، يتم التصفية به تلقائياً
        if (!filter.BranchId.HasValue)
        {
            filter.BranchId = _currentUserService.GetCurrentBranchId();
        }

        var result = await _quotationService.GetPagedAsync(filter);
        return Ok(ApiResponse<PagedResult<QuotationDto>>.Succeeded(result, "تم جلب عروض الأسعار بنجاح"));
    }

    /// <summary>
    /// جلب تفاصيل عرض سعر محدد
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var quotation = await _quotationService.GetByIdAsync(id);
        if (quotation == null)
            return NotFound(ApiResponse<QuotationDto>.Failed("عرض السعر غير موجود"));

        return Ok(ApiResponse<QuotationDto>.Succeeded(quotation, "تم جلب تفاصيل عرض السعر"));
    }

    /// <summary>
    /// جلب بيانات طباعة عرض السعر بصيغة رسمية A4
    /// </summary>
    [HttpGet("{id}/print")]
    public async Task<IActionResult> GetPrintData(int id)
    {
        var printData = await _quotationService.GetPrintDataAsync(id);
        if (printData == null)
            return NotFound(ApiResponse<QuotationPrintDto>.Failed("عرض السعر غير موجود"));

        return Ok(ApiResponse<QuotationPrintDto>.Succeeded(printData, "تم تجهيز بيانات طباعة عرض السعر"));
    }

    /// <summary>
    /// إنشاء عرض سعر جديد
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateQuotationDto dto)
    {
        var userId = GetUserId();
        if (!dto.BranchId.HasValue)
        {
            dto.BranchId = _currentUserService.GetCurrentBranchId();
        }

        var created = await _quotationService.CreateAsync(dto, userId);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, ApiResponse<QuotationDto>.Succeeded(created, "تم إنشاء عرض السعر بنجاح"));
    }

    /// <summary>
    /// تعديل عرض سعر
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateQuotationDto dto)
    {
        var userId = GetUserId();
        var updated = await _quotationService.UpdateAsync(id, dto, userId);
        return Ok(ApiResponse<QuotationDto>.Succeeded(updated, "تم تحديث عرض السعر بنجاح"));
    }

    /// <summary>
    /// حذف عرض سعر (Soft Delete)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _quotationService.DeleteAsync(id);
        return Ok(ApiResponse<bool>.Succeeded(success, "تم حذف عرض السعر بنجاح"));
    }

    /// <summary>
    /// تغيير حالة عرض السعر (إرسال، قبول، رفض، انتهاء صلاحية)
    /// </summary>
    [HttpPatch("{id}/status")]
    public async Task<IActionResult> ChangeStatus(int id, [FromQuery] QuotationStatus status)
    {
        var userId = GetUserId();
        var updated = await _quotationService.ChangeStatusAsync(id, status, userId);
        return Ok(ApiResponse<QuotationDto>.Succeeded(updated, "تم تغيير حالة عرض السعر بنجاح"));
    }

    /// <summary>
    /// تحويل عرض السعر إلى فاتورة مبيعات فعلية رسمية بنقرة واحدة
    /// يتم خصم الكميات من المخزون وإنشاء القيود المحاسبية وتوليد رمز ZATCA QR
    /// </summary>
    [HttpPost("{id}/convert-to-invoice")]
    public async Task<IActionResult> ConvertToSaleInvoice(int id, [FromBody] ConvertQuotationToInvoiceDto dto)
    {
        var userId = GetUserId();
        var invoice = await _quotationService.ConvertToSaleInvoiceAsync(id, dto, userId);
        return Ok(ApiResponse<SaleInvoiceDto>.Succeeded(invoice, $"تم تحويل عرض السعر إلى فاتورة مبيعات رقم {invoice.SaleInvoiceNumber} بنجاح"));
    }
}
