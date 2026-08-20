using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartPharmacySystem.Application.DTOs.Pricelists;
using SmartPharmacySystem.Application.IServices;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Application.Wrappers;

namespace SmartPharmacySystem.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PricelistController : ControllerBase
{
    private readonly IPricelistService _pricelistService;
    private readonly ICurrentUserService _currentUserService;

    public PricelistController(IPricelistService pricelistService, ICurrentUserService currentUserService)
    {
        _pricelistService = pricelistService;
        _currentUserService = currentUserService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool? isActive = null)
    {
        var result = await _pricelistService.GetAllAsync(isActive);
        return Ok(ApiResponse<IEnumerable<PricelistDto>>.Succeeded(result, "تم جلب قوائم الأسعار بنجاح"));
    }

    [HttpGet("select")]
    public async Task<IActionResult> GetSelectList()
    {
        var result = await _pricelistService.GetSelectListAsync();
        return Ok(ApiResponse<IEnumerable<PricelistSelectDto>>.Succeeded(result, "تم جلب قوائم الأسعار"));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _pricelistService.GetByIdAsync(id);
        if (result == null) return NotFound(ApiResponse<string>.Failed("قائمة الأسعار غير موجودة"));
        return Ok(ApiResponse<PricelistDto>.Succeeded(result, "تم جلب قائمة الأسعار بنجاح"));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePricelistDto dto)
    {
        var userId = _currentUserService.UserId ?? 0;
        var result = await _pricelistService.CreateAsync(dto, userId);
        return Ok(ApiResponse<PricelistDto>.Succeeded(result, "تم إنشاء قائمة الأسعار بنجاح"));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdatePricelistDto dto)
    {
        var userId = _currentUserService.UserId ?? 0;
        var result = await _pricelistService.UpdateAsync(id, dto, userId);
        return Ok(ApiResponse<PricelistDto>.Succeeded(result, "تم تحديث قائمة الأسعار بنجاح"));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _pricelistService.DeleteAsync(id);
        return Ok(ApiResponse<string>.Succeeded("تم حذف قائمة الأسعار بنجاح", "تم الحذف"));
    }

    /// <summary>
    /// Get the effective discount % for a specific medicine under a specific pricelist.
    /// Used by POS to show the applicable discount when a customer with a pricelist is selected.
    /// </summary>
    [HttpGet("{pricelistId}/discount/{medicineId}")]
    public async Task<IActionResult> GetDiscount(int pricelistId, int medicineId)
    {
        var discount = await _pricelistService.GetDiscountPercentageAsync(pricelistId, medicineId);
        return Ok(ApiResponse<decimal>.Succeeded(discount, "تم حساب الخصم"));
    }
}
