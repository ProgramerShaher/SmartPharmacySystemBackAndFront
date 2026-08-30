namespace SmartPharmacySystem.Core.Interfaces;

/// <summary>
/// Defines the contract for the hardware-binding licensing service.
/// Handles Machine ID generation, license key validation, and key generation.
/// </summary>
public interface ILicenseService
{
    /// <summary>
    /// Reads the motherboard serial number via WMI and generates a
    /// formatted, hashed Machine ID (e.g., "ABCD-1234-EFGH-5678").
    /// Falls back to Environment.MachineName if WMI is unavailable.
    /// </summary>
    string GetMachineId();

    /// <summary>
    /// Validates a given license key against the current machine's ID
    /// using HMAC-SHA256. Returns true only if the key is authentic
    /// and was generated for this exact machine.
    /// </summary>
    /// <param name="licenseKey">The license key string to validate.</param>
    bool ValidateLicense(string licenseKey);

    /// <summary>
    /// [DEVELOPER USE ONLY] Generates a valid license key for a given
    /// machine ID. This method should only be called from the Keygen tool,
    /// never exposed publicly.
    /// </summary>
    /// <param name="machineId">The formatted Machine ID from the client.</param>
    string GenerateKeyForClient(string machineId);
}
