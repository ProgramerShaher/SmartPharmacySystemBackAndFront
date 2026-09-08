using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartPharmacySystem.Application.DTOs.License;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Interfaces;
using SmartPharmacySystem.Infrastructure.Data;

namespace SmartPharmacySystem.Controllers;

/// <summary>
/// Manages hardware-bound software licensing.
///
/// Endpoints:
///   GET  /api/license/status   — Returns current machine ID and activation state.
///   POST /api/license/activate — Validates a license key and activates the system.
///
/// Note: These endpoints are intentionally NOT protected by [Authorize] so that
/// the lock-screen can reach them before the user logs in.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public sealed class LicenseController : ControllerBase
{
    private readonly ILicenseService      _licenseService;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<LicenseController> _logger;

    public LicenseController(
        ILicenseService           licenseService,
        ApplicationDbContext      context,
        ILogger<LicenseController> logger)
    {
        _licenseService = licenseService;
        _context        = context;
        _logger         = logger;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // GET /api/license/status
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns the machine's unique ID and whether a valid license key is stored.
    /// The Angular guard calls this on startup to decide if the lock-screen should appear.
    /// </summary>
    [HttpGet("status")]
    [ProducesResponseType(typeof(LicenseStatusDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStatus()
    {
        var machineId = _licenseService.GetMachineId();

        // Check if a license key is saved in PharmacySettings
        var settings = await _context.PharmacySettings.FirstOrDefaultAsync();

        var isActivated = settings is not null
            && !string.IsNullOrWhiteSpace(settings.LicenseKey)
            && _licenseService.ValidateLicense(settings.LicenseKey);

        return Ok(new LicenseStatusDto(machineId, isActivated));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // POST /api/license/activate
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Accepts a license key, validates it against the current machine ID,
    /// and if valid, persists it to the database.
    /// </summary>
    [HttpPost("activate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Activate([FromBody] ActivateLicenseDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto?.LicenseKey))
            return BadRequest(new { message = "مفتاح الترخيص لا يمكن أن يكون فارغًا." });

        // Validate against the current machine hardware
        if (!_licenseService.ValidateLicense(dto.LicenseKey))
        {
            _logger.LogWarning("[License] Invalid activation attempt on machine {MachineId}.",
                _licenseService.GetMachineId());

            return BadRequest(new { message = "مفتاح الترخيص غير صالح أو لا يطابق هذا الجهاز." });
        }

        try
        {
            // Persist the validated key to PharmacySettings
            var settings = await _context.PharmacySettings.FirstOrDefaultAsync();

            if (settings is null)
            {
                // Bootstrap with a minimal settings record if one doesn't exist yet
                settings = new PharmacySettings
                {
                    PharmacyName = "الصيدلية الذكية",
                    BaseCurrency = "ريال يمني",
                    LicenseKey   = dto.LicenseKey.Trim()
                };
                await _context.PharmacySettings.AddAsync(settings);
            }
            else
            {
                settings.LicenseKey = dto.LicenseKey.Trim();
                _context.PharmacySettings.Update(settings);
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("[License] System successfully activated on machine {MachineId}.",
                _licenseService.GetMachineId());

            return Ok(new { message = "تم تفعيل الترخيص بنجاح. مرحبًا بك في النظام!" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[License] Failed to persist license key to database.");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "حدث خطأ داخلي أثناء حفظ الترخيص. يرجى المحاولة مرة أخرى." });
        }
    }
}
