namespace SmartPharmacySystem.Application.IServices;

public interface IKeyManagementService
{
    /// <summary>
    /// Loads the AES key from the configured key file path using DPAPI decryption.
    /// Returns null if the file does not exist.
    /// </summary>
    byte[]? LoadKey();

    /// <summary>
    /// Encrypts the raw AES key using DPAPI and saves it to the configured key file path.
    /// </summary>
    void SaveKey(byte[] rawKey);

    /// <summary>
    /// Exports the AES key as a Base64 string for disaster recovery.
    /// Requires extreme security measures around its caller.
    /// </summary>
    string? ExportKeyBase64();

    /// <summary>
    /// Imports a Base64 encoded recovery key and saves it locally using DPAPI.
    /// </summary>
    void ImportKeyBase64(string base64Key);

    /// <summary>
    /// Generates a new key using IEncryptionService and saves it using DPAPI.
    /// </summary>
    void InitializeNewKey();
}
