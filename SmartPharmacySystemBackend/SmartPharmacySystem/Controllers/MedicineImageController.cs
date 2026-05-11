using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartPharmacySystem.Application.Wrappers;
using System.Text.RegularExpressions;

namespace SmartPharmacySystem.Controllers;

/// <summary>
/// Medicine Image Controller
/// Stores images in organized folder hierarchy:
/// wwwroot/images/medicines/{category}/{subCategory}/{medicineName}_{guid}.{ext}
/// Example: images/medicines/pills/antibiotics/amoxicillin_a1b2c3.jpg
/// </summary>
[ApiController]
[Route("api/medicine-images")]
[Authorize(Roles = "Admin,Pharmacist")]
public class MedicineImageController : ControllerBase
{
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<MedicineImageController> _logger;

    // Predefined category slugs (Arabic → folder name)
    private static readonly Dictionary<string, string> CategorySlugs = new(StringComparer.OrdinalIgnoreCase)
    {
        { "tablets", "tablets" },         { "حبوب", "tablets" },
        { "capsules", "capsules" },       { "كبسولات", "capsules" },
        { "syrups", "syrups" },           { "شرابات", "syrups" },
        { "injections", "injections" },   { "حقن", "injections" },
        { "creams", "creams" },           { "كريمات", "creams" },
        { "drops", "drops" },             { "قطرات", "drops" },
        { "inhalers", "inhalers" },       { "بخاخات", "inhalers" },
        { "vitamins", "vitamins" },       { "فيتامينات", "vitamins" },
        { "cosmetics", "cosmetics" },     { "مستحضرات تجميل", "cosmetics" },
        { "medical-devices", "medical-devices" }, { "أجهزة طبية", "medical-devices" },
        { "other", "other" },             { "أخرى", "other" },
    };

    public MedicineImageController(IWebHostEnvironment env, ILogger<MedicineImageController> logger)
    {
        _env = env;
        _logger = logger;
    }

    /// <summary>
    /// Upload a medicine image with organized folder structure.
    /// </summary>
    /// <param name="file">Image file (jpg/png/webp, max 5MB)</param>
    /// <param name="category">Category folder (e.g. tablets, capsules, syrups, حبوب)</param>
    /// <param name="subCategory">Sub-category folder (e.g. antibiotics, painkillers)</param>
    /// <param name="medicineName">Medicine name used in filename (e.g. amoxicillin)</param>
    [HttpPost("upload")]
    public async Task<IActionResult> Upload(
        IFormFile file,
        [FromQuery] string category = "other",
        [FromQuery] string subCategory = "general",
        [FromQuery] string medicineName = "medicine")
    {
        if (file == null || file.Length == 0)
            return BadRequest(ApiResponse<object>.Failed("لم يتم اختيار ملف"));

        // Validate file type
        var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowed.Contains(ext))
            return BadRequest(ApiResponse<object>.Failed("نوع الملف غير مدعوم. المسموح: jpg, png, webp"));

        // Validate size (max 5 MB)
        if (file.Length > 5 * 1024 * 1024)
            return BadRequest(ApiResponse<object>.Failed("حجم الصورة يجب أن لا يتجاوز 5 ميغابايت"));

        try
        {
            // Resolve category folder slug
            var categorySlug = CategorySlugs.TryGetValue(category.Trim(), out var slug)
                ? slug : Slugify(category);

            // Build safe folder path
            var subSlug = Slugify(subCategory);
            var medicineSlug = Slugify(medicineName);

            // Create folder if not exists
            var folderPath = Path.Combine(_env.WebRootPath, "images", "medicines", categorySlug, subSlug);
            Directory.CreateDirectory(folderPath);

            // Generate unique filename: medicinename_shortguid.ext
            var shortGuid = Guid.NewGuid().ToString("N")[..8]; // e.g. a1b2c3d4
            var fileName = $"{medicineSlug}_{shortGuid}{ext}";
            var fullPath = Path.Combine(folderPath, fileName);

            await using var stream = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(stream);

            // Build public URL
            var request = HttpContext.Request;
            var relativeUrl = $"/images/medicines/{categorySlug}/{subSlug}/{fileName}";
            var absoluteUrl = $"{request.Scheme}://{request.Host}{relativeUrl}";

            _logger.LogInformation("Medicine image uploaded: {Url}", absoluteUrl);

            return Ok(ApiResponse<object>.Succeeded(new
            {
                imageUrl = absoluteUrl,
                relativePath = relativeUrl,
                fileName,
                category = categorySlug,
                subCategory = subSlug
            }, "تم رفع الصورة بنجاح"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading medicine image");
            return StatusCode(500, ApiResponse<object>.Failed("حدث خطأ أثناء رفع الصورة"));
        }
    }

    /// <summary>
    /// List all images for a specific category and sub-category.
    /// </summary>
    [HttpGet("list")]
    public IActionResult List([FromQuery] string category = "other", [FromQuery] string subCategory = "general")
    {
        var categorySlug = CategorySlugs.TryGetValue(category.Trim(), out var slug) ? slug : Slugify(category);
        var subSlug = Slugify(subCategory);
        var folderPath = Path.Combine(_env.WebRootPath, "images", "medicines", categorySlug, subSlug);

        if (!Directory.Exists(folderPath))
            return Ok(ApiResponse<object>.Succeeded(new { images = Array.Empty<string>() }, "المجلد فارغ"));

        var request = HttpContext.Request;
        var images = Directory.GetFiles(folderPath)
            .Select(f => $"{request.Scheme}://{request.Host}/images/medicines/{categorySlug}/{subSlug}/{Path.GetFileName(f)}")
            .ToList();

        return Ok(ApiResponse<object>.Succeeded(new { images, count = images.Count }, "تم جلب الصور"));
    }

    /// <summary>
    /// Delete a specific medicine image by its full relative path.
    /// Example: /images/medicines/tablets/antibiotics/amox_a1b2.jpg
    /// </summary>
    [HttpDelete]
    public IActionResult Delete([FromQuery] string relativePath)
    {
        // Security: only allow paths inside images/medicines/
        if (string.IsNullOrWhiteSpace(relativePath) ||
            !relativePath.StartsWith("/images/medicines/") ||
            relativePath.Contains(".."))
        {
            return BadRequest(ApiResponse<object>.Failed("المسار غير صالح"));
        }

        var safePath = relativePath.Replace('/', Path.DirectorySeparatorChar).TrimStart(Path.DirectorySeparatorChar);
        var fullPath = Path.Combine(_env.WebRootPath, safePath);

        if (!System.IO.File.Exists(fullPath))
            return NotFound(ApiResponse<object>.Failed("الصورة غير موجودة"));

        System.IO.File.Delete(fullPath);
        return Ok(ApiResponse<object>.Succeeded(null, "تم حذف الصورة بنجاح"));
    }

    // ==================== Helpers ====================

    /// <summary>
    /// Converts any string to a safe URL/folder slug.
    /// "Amoxicillin 500mg" → "amoxicillin-500mg"
    /// </summary>
    private static string Slugify(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return "general";
        var lower = input.Trim().ToLowerInvariant();
        // Replace spaces and underscores with hyphens
        var slug = Regex.Replace(lower, @"[\s_]+", "-");
        // Remove anything that's not alphanumeric or hyphen
        slug = Regex.Replace(slug, @"[^\w\-]", "");
        // Collapse multiple hyphens
        slug = Regex.Replace(slug, @"-{2,}", "-").Trim('-');
        return string.IsNullOrEmpty(slug) ? "general" : slug;
    }
}
