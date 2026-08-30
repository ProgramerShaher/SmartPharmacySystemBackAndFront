namespace SmartPharmacySystem.Application.DTOs.License;

/// <summary>
/// Returned by GET /api/license/status — shows the machine ID and current activation state.
/// </summary>
public sealed record LicenseStatusDto(
    string MachineId,
    bool   IsActivated
);

/// <summary>
/// Accepted by POST /api/license/activate — the raw license key submitted by the user.
/// </summary>
public sealed record ActivateLicenseDto(
    string LicenseKey
);
