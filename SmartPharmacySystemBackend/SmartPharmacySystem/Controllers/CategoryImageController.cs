using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartPharmacySystem.Application.Wrappers;
using System.Text.RegularExpressions;

namespace SmartPharmacySystem.Controllers;

/// <summary>
/// Category Image Controller
/// Stores images in: wwwroot/images/categories/{categoryName}_{guid}.{ext}
/// </summary>
[ApiController]
[Route("api/category-images")]
[Authorize(Roles = "Admin,Pharmacist")]
public class CategoryImageController : ControllerBase
{
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<CategoryImageController> _logger;

    public CategoryImageController(IWebHostEnvironment env, ILogger<CategoryImageController> logger)
    {
        _env = env;
        _logger = logger;
    }

    /// <summary>
    /// Upload a category image.
    /// </summary>
    [HttpPost("upload")]
    public async Task<IActionResult> Upload(IFormFile file, [FromQuery] string categoryName = "category")
    {
        if (file == null || file.Length == 0)
            return BadRequest(ApiResponse<object>.Failed("لم يتم اختيار ملف"));

        var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowed.Contains(ext))
            return BadRequest(ApiResponse<object>.Failed("نوع الملف غير مدعوم. المسموح: jpg, png, webp"));

        if (file.Length > 2 * 1024 * 1024) // 2MB is enough for icons
            return BadRequest(ApiResponse<object>.Failed("حجم الصورة يجب أن لا يتجاوز 2 ميغابايت"));

        try
        {
            var folderPath = Path.Combine(_env.WebRootPath, "images", "categories");
            Directory.CreateDirectory(folderPath);

            var slug = Slugify(categoryName);
            var shortGuid = Guid.NewGuid().ToString("N")[..6];
            var fileName = $"{slug}_{shortGuid}{ext}";
            var fullPath = Path.Combine(folderPath, fileName);

            await using var stream = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(stream);

            var request = HttpContext.Request;
            var relativeUrl = $"/images/categories/{fileName}";
            var absoluteUrl = $"{request.Scheme}://{request.Host}{relativeUrl}";

            return Ok(ApiResponse<object>.Succeeded(new
            {
                imageUrl = absoluteUrl,
                relativePath = relativeUrl,
                fileName
            }, "تم رفع صورة الصنف بنجاح"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading category image");
            return StatusCode(500, ApiResponse<object>.Failed("حدث خطأ أثناء رفع الصورة"));
        }
    }

    private static string Slugify(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return "category";
        var lower = input.Trim().ToLowerInvariant();
        var slug = Regex.Replace(lower, @"[\s_]+", "-");
        slug = Regex.Replace(slug, @"[^\w\-]", "");
        return string.IsNullOrEmpty(slug) ? "category" : slug;
    }
}
