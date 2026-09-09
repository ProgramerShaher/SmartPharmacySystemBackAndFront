using Microsoft.AspNetCore.Mvc;
using SmartPharmacySystem.Application.DTOs.Settings;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Controllers;

/// <summary>
/// إدارة وتكوين أنماط الأنشطة التجارية وتخصيص إعدادات النظام
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class BusinessProfileController : ControllerBase
{
    private readonly IBusinessProfileService _profileService;

    public BusinessProfileController(IBusinessProfileService profileService)
    {
        _profileService = profileService;
    }

    /// <summary>
    /// استرجاع ملف النشاط التجاري الفعال والمعتمد حالياً
    /// </summary>
    [HttpGet("current")]
    public async Task<ActionResult<BusinessProfileDto>> GetCurrentProfile()
    {
        var profile = await _profileService.GetActiveProfileAsync();
        return Ok(profile);
    }

    /// <summary>
    /// استرجاع القوالب المسبقة لجميع الأنشطة التجارية الـ 10 المدعومة
    /// </summary>
    [HttpGet("presets")]
    public ActionResult<IEnumerable<BusinessProfileDto>> GetAllPresets()
    {
        var presets = _profileService.GetAllPresetProfiles();
        return Ok(presets);
    }

    /// <summary>
    /// استرجاع القالب المسبق لنشاط تجاري معين
    /// </summary>
    [HttpGet("presets/{type}")]
    public ActionResult<BusinessProfileDto> GetPresetByType(BusinessType type)
    {
        var preset = _profileService.GetPresetForType(type);
        return Ok(preset);
    }

    /// <summary>
    /// تبديل نوع النشاط التجاري للنظام وتطبيق قالبه الافتراضي
    /// </summary>
    [HttpPost("switch/{type}")]
    public async Task<ActionResult<BusinessProfileDto>> SwitchBusinessType(BusinessType type)
    {
        var profile = await _profileService.SwitchBusinessTypeAsync(type);
        return Ok(profile);
    }

    /// <summary>
    /// تعديل وتخصيص إعدادات ملف النشاط التجاري النشط
    /// </summary>
    [HttpPut("current")]
    public async Task<ActionResult<BusinessProfileDto>> UpdateCustomProfile([FromBody] UpdateBusinessProfileDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var updated = await _profileService.UpdateCustomProfileAsync(dto);
        return Ok(updated);
    }
}
