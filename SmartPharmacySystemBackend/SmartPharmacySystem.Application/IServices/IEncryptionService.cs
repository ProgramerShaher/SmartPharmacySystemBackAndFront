using System.Threading.Tasks;

namespace SmartPharmacySystem.Application.IServices;

public interface IEncryptionService
{
    /// <summary>
    /// Generates a new 256-bit AES key.
    /// </summary>
    /// <returns>A 32-byte cryptographic key.</returns>
    byte[] GenerateAesKey();

    /// <summary>
    /// Encrypts a large file using Chunk-based AES-256-GCM.
    /// The file format uses AAD to protect the header and chunk indexes.
    /// </summary>
    Task EncryptFileAsync(string inputFilePath, string outputFilePath, byte[] key, string occurrenceId);

    /// <summary>
    /// Decrypts a file that was encrypted using EncryptFileAsync.
    /// Performs strict AAD authentication to prevent chunk reordering or truncation.
    /// </summary>
    Task DecryptFileAsync(string inputFilePath, string outputFilePath, byte[] key, string occurrenceId);
}
