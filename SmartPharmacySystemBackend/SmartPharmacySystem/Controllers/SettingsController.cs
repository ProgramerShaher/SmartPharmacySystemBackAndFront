using Microsoft.AspNetCore.Mvc;
using SmartPharmacySystem.Application.DTOs.Settings;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Interfaces;
using SmartPharmacySystem.Infrastructure.Data;

namespace SmartPharmacySystem.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SettingsController : ControllerBase
{
    private readonly IPharmacySettingsService _settingsService;
    private readonly IWebHostEnvironment _env;
    private readonly ApplicationDbContext _context;

    public SettingsController(IPharmacySettingsService settingsService, IWebHostEnvironment env, ApplicationDbContext context)
    {
        _settingsService = settingsService;
        _env = env;
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetSettings()
    {
        var settings = await _settingsService.GetSettingsAsync();
        return Ok(settings);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateSettings([FromBody] UpdatePharmacySettingsDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var updatedSettings = await _settingsService.UpdateSettingsAsync(dto);
        return Ok(updatedSettings);
    }

    [HttpPost("logo")]
    public async Task<IActionResult> UploadLogo(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("لم يتم تحديد أي ملف");

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (!allowedExtensions.Contains(extension))
            return BadRequest("صيغة الملف غير مدعومة. يرجى رفع صورة (jpg, jpeg, png, webp)");

        if (file.Length > 2 * 1024 * 1024) // 2MB Limit
            return BadRequest("حجم الصورة يجب أن لا يتجاوز 2 ميجابايت");

        try
        {
            var uploadsFolder = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads", "logos");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = $"pharmacy_logo_{DateTime.Now.Ticks}{extension}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            var logoUrl = $"/uploads/logos/{uniqueFileName}";

            // Update the database with the new URL
            var settings = _context.PharmacySettings.FirstOrDefault();
            if (settings != null)
            {
                settings.LogoUrl = logoUrl;
                _context.Update(settings);
                await _context.SaveChangesAsync();
            }

            return Ok(new { LogoUrl = logoUrl, Message = "تم رفع الشعار بنجاح" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"حدث خطأ داخلي أثناء رفع الشعار: {ex.Message}");
        }
    }
}
