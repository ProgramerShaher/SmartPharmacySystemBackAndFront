using System;
using System.IO;
using System.Security.Cryptography;
using Microsoft.Extensions.Configuration;
using SmartPharmacySystem.Application.IServices;

namespace SmartPharmacySystem.Infrastructure.Services;

public class KeyManagementService : IKeyManagementService
{
    private readonly IConfiguration _configuration;
    private readonly IEncryptionService _encryptionService;
    
    // Entropy to add additional security to DPAPI
    private static readonly byte[] Entropy = System.Text.Encoding.UTF8.GetBytes("SmartPharmacySystem.Backup.Key.Entropy");

    public KeyManagementService(IConfiguration configuration, IEncryptionService encryptionService)
    {
        _configuration = configuration;
        _encryptionService = encryptionService;
    }

    private string GetKeyFilePath()
    {
        var path = _configuration["BackupSettings:EncryptionKeyPath"];
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new InvalidOperationException("BackupSettings:EncryptionKeyPath is not configured.");
        }
        return path;
    }

    public byte[]? LoadKey()
    {
        var path = GetKeyFilePath();
        if (!File.Exists(path)) return null;

        var encryptedKey = File.ReadAllBytes(path);
        
        try
        {
            // Decrypt using LocalMachine scope so the Windows Service can read it.
            return ProtectedData.Unprotect(encryptedKey, Entropy, DataProtectionScope.LocalMachine);
        }
        catch (CryptographicException ex)
        {
            throw new InvalidOperationException("Failed to decrypt the backup key. This could happen if the server OS was reinstalled or moved.", ex);
        }
    }

    public void SaveKey(byte[] rawKey)
    {
        if (rawKey == null || rawKey.Length != 32)
        {
            throw new ArgumentException("Invalid AES key length. Expected 32 bytes.");
        }

        var path = GetKeyFilePath();
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var encryptedKey = ProtectedData.Protect(rawKey, Entropy, DataProtectionScope.LocalMachine);
        
        // Write securely (overwrite)
        File.WriteAllBytes(path, encryptedKey);
    }

    public string? ExportKeyBase64()
    {
        var key = LoadKey();
        if (key == null) return null;
        
        return Convert.ToBase64String(key);
    }

    public void ImportKeyBase64(string base64Key)
    {
        if (string.IsNullOrWhiteSpace(base64Key)) throw new ArgumentException("Key cannot be empty.");
        
        byte[] rawKey;
        try
        {
            rawKey = Convert.FromBase64String(base64Key);
        }
        catch (FormatException)
        {
            throw new ArgumentException("Key is not a valid Base64 string.");
        }

        if (rawKey.Length != 32)
        {
            throw new ArgumentException("The imported key is not a valid 256-bit AES key (must be 32 bytes).");
        }

        SaveKey(rawKey);
    }

    public void InitializeNewKey()
    {
        var path = GetKeyFilePath();
        if (File.Exists(path))
        {
            throw new InvalidOperationException("A backup key already exists. Cannot initialize a new one.");
        }

        var newKey = _encryptionService.GenerateAesKey();
        SaveKey(newKey);
    }
}
