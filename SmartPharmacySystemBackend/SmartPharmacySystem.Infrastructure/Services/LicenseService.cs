using System.Management;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using SmartPharmacySystem.Core.Interfaces;

namespace SmartPharmacySystem.Infrastructure.Services;

/// <summary>
/// Hardware-binding license service.
/// Machine ID = SHA256(MotherboardSerial + SECRET_SALT), formatted as XXXX-XXXX-XXXX-XXXX.
/// License Key = HMACSHA256(MachineId, SECRET_SALT), encoded as Base64.
/// </summary>
public sealed class LicenseService : ILicenseService
{
    // ─────────────────────────────────────────────────────────────────────────
    // IMPORTANT: Change this salt before shipping. Keep it secret.
    // Never commit the real value to a public repository.
    // ─────────────────────────────────────────────────────────────────────────
    private const string SecretSalt = "SP@SmartPharmacy#2025!SecureKey$";

    private readonly ILogger<LicenseService> _logger;

    public LicenseService(ILogger<LicenseService> logger)
    {
        _logger = logger;
    }

    // ─── Public API ──────────────────────────────────────────────────────────

    /// <inheritdoc />
    public string GetMachineId()
    {
        var rawSerial = GetMotherboardSerial();
        return BuildFormattedId(rawSerial + SecretSalt);
    }

    /// <inheritdoc />
    public bool ValidateLicense(string licenseKey)
    {
        if (string.IsNullOrWhiteSpace(licenseKey))
            return false;

        try
        {
            var currentMachineId = GetMachineId();
            var expectedKey      = ComputeHmacSha256(currentMachineId, SecretSalt);

            // Constant-time comparison to prevent timing attacks
            return CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(expectedKey),
                Encoding.UTF8.GetBytes(licenseKey.Trim()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[LicenseService] License validation failed unexpectedly.");
            return false;
        }
    }

    /// <inheritdoc />
    public string GenerateKeyForClient(string machineId)
    {
        if (string.IsNullOrWhiteSpace(machineId))
            throw new ArgumentException("Machine ID must not be empty.", nameof(machineId));

        return ComputeHmacSha256(machineId.Trim(), SecretSalt);
    }

    // ─── Private Helpers ─────────────────────────────────────────────────────

    /// <summary>
    /// Reads the motherboard serial number via WMI (Windows).
    /// Falls back to <see cref="Environment.MachineName"/> if WMI fails or
    /// if the system runs on a non-Windows OS.
    /// </summary>
    private string GetMotherboardSerial()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_BaseBoard");
            using var results  = searcher.Get();

            foreach (ManagementObject mo in results)
            {
                var serial = mo["SerialNumber"]?.ToString()?.Trim();

                // Some motherboards return "Default string" or empty — treat as invalid
                if (!string.IsNullOrWhiteSpace(serial) &&
                    !serial.Equals("Default string", StringComparison.OrdinalIgnoreCase) &&
                    !serial.Equals("To be filled by O.E.M.", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogInformation("[LicenseService] Motherboard serial retrieved via WMI.");
                    return serial;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[LicenseService] WMI query failed. Falling back to MachineName.");
        }

        // Fallback: use machine name (less unique, but better than nothing)
        var fallback = Environment.MachineName;
        _logger.LogWarning("[LicenseService] Using MachineName as fallback hardware identifier: {Name}", fallback);
        return fallback;
    }

    /// <summary>
    /// Produces a 16-character uppercase hex string from SHA256 of <paramref name="input"/>,
    /// then formats it as XXXX-XXXX-XXXX-XXXX.
    /// </summary>
    private static string BuildFormattedId(string input)
    {
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));

        // Take the first 8 bytes → 16 hex chars → 4 groups of 4
        var hex = Convert.ToHexString(hashBytes)[..16].ToUpperInvariant();

        return $"{hex[0..4]}-{hex[4..8]}-{hex[8..12]}-{hex[12..16]}";
    }

    /// <summary>
    /// Computes HMAC-SHA256(<paramref name="data"/>, <paramref name="key"/>)
    /// and returns it as a Base64 string.
    /// </summary>
    private static string ComputeHmacSha256(string data, string key)
    {
        var keyBytes  = Encoding.UTF8.GetBytes(key);
        var dataBytes = Encoding.UTF8.GetBytes(data);

        var hmac = new HMACSHA256(keyBytes);
        var hash = hmac.ComputeHash(dataBytes);
        return Convert.ToBase64String(hash);
    }
}
