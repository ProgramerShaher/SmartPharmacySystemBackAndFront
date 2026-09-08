using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using SmartPharmacySystem.Application.IServices;

namespace SmartPharmacySystem.Infrastructure.Services;

public class EncryptionService : IEncryptionService
{
    private const int ChunkSize = 4 * 1024 * 1024; // 4MB
    private const int NonceSize = 12;
    private const int TagSize = 16;
    private static readonly byte[] MagicBytes = Encoding.UTF8.GetBytes("SPSBAK");
    private static readonly byte[] VersionBytes = Encoding.UTF8.GetBytes("01");

    public byte[] GenerateAesKey()
    {
        var key = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(key);
        return key;
    }

    private byte[] CreateAad(long chunkIndex, int ciphertextLength)
    {
        // AAD = Magic (6) + Version (2) + ChunkIndex (8) + CiphertextLength (4) = 20 bytes
        var aad = new byte[MagicBytes.Length + VersionBytes.Length + 8 + 4];
        int offset = 0;
        
        Buffer.BlockCopy(MagicBytes, 0, aad, offset, MagicBytes.Length);
        offset += MagicBytes.Length;
        
        Buffer.BlockCopy(VersionBytes, 0, aad, offset, VersionBytes.Length);
        offset += VersionBytes.Length;
        
        Buffer.BlockCopy(BitConverter.GetBytes(chunkIndex), 0, aad, offset, 8);
        offset += 8;
        
        Buffer.BlockCopy(BitConverter.GetBytes(ciphertextLength), 0, aad, offset, 4);
        
        return aad;
    }

    public async Task EncryptFileAsync(string inputFilePath, string outputFilePath, byte[] key, string occurrenceId)
    {
        if (key.Length != 32) throw new ArgumentException("Key must be 32 bytes for AES-256.");

        using var inputStream = new FileStream(inputFilePath, FileMode.Open, FileAccess.Read, FileShare.Read, ChunkSize, true);
        using var outputStream = new FileStream(outputFilePath, FileMode.Create, FileAccess.Write, FileShare.None, ChunkSize, true);

        // Write Header
        await outputStream.WriteAsync(MagicBytes, 0, MagicBytes.Length);
        await outputStream.WriteAsync(VersionBytes, 0, VersionBytes.Length);

        var buffer = new byte[ChunkSize];
        long chunkIndex = 0;
        int bytesRead;

        using var aesGcm = new AesGcm(key, TagSize);

        while ((bytesRead = await inputStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
        {
            var nonce = new byte[NonceSize];
            RandomNumberGenerator.Fill(nonce);

            var tag = new byte[TagSize];
            var ciphertext = new byte[bytesRead];

            var aad = CreateAad(chunkIndex, bytesRead);

            // In AES-GCM, plaintext and ciphertext are the same length
            aesGcm.Encrypt(nonce, buffer.AsSpan(0, bytesRead), ciphertext, tag, aad);

            // Write chunk metadata and payload
            await outputStream.WriteAsync(nonce, 0, nonce.Length);
            
            var lengthBytes = BitConverter.GetBytes(bytesRead);
            await outputStream.WriteAsync(lengthBytes, 0, lengthBytes.Length);
            
            await outputStream.WriteAsync(ciphertext, 0, ciphertext.Length);
            await outputStream.WriteAsync(tag, 0, tag.Length);

            chunkIndex++;
        }
    }

    public async Task DecryptFileAsync(string inputFilePath, string outputFilePath, byte[] key, string occurrenceId)
    {
        if (key.Length != 32) throw new ArgumentException("Key must be 32 bytes for AES-256.");

        using var inputStream = new FileStream(inputFilePath, FileMode.Open, FileAccess.Read, FileShare.Read, ChunkSize, true);
        using var outputStream = new FileStream(outputFilePath, FileMode.Create, FileAccess.Write, FileShare.None, ChunkSize, true);

        var header = new byte[MagicBytes.Length + VersionBytes.Length];
        int headerRead = await inputStream.ReadAsync(header, 0, header.Length);
        if (headerRead != header.Length) throw new CryptographicException("File is too short or missing header.");

        for (int i = 0; i < MagicBytes.Length; i++)
        {
            if (header[i] != MagicBytes[i]) throw new CryptographicException("Invalid Magic Header.");
        }
        for (int i = 0; i < VersionBytes.Length; i++)
        {
            if (header[MagicBytes.Length + i] != VersionBytes[i]) throw new CryptographicException("Invalid Version.");
        }

        long chunkIndex = 0;
        using var aesGcm = new AesGcm(key, TagSize);

        var nonce = new byte[NonceSize];
        var lengthBytes = new byte[4];
        var tag = new byte[TagSize];

        while (true)
        {
            int nonceRead = await inputStream.ReadAsync(nonce, 0, nonce.Length);
            if (nonceRead == 0) break; // EOF
            if (nonceRead != nonce.Length) throw new CryptographicException("Unexpected EOF while reading Nonce.");

            int lenRead = await inputStream.ReadAsync(lengthBytes, 0, lengthBytes.Length);
            if (lenRead != lengthBytes.Length) throw new CryptographicException("Unexpected EOF while reading CiphertextLength.");
            
            int ciphertextLength = BitConverter.ToInt32(lengthBytes, 0);
            if (ciphertextLength <= 0 || ciphertextLength > ChunkSize) throw new CryptographicException("Invalid Chunk Length.");

            var ciphertext = new byte[ciphertextLength];
            int ctRead = 0;
            while (ctRead < ciphertextLength)
            {
                int read = await inputStream.ReadAsync(ciphertext, ctRead, ciphertextLength - ctRead);
                if (read == 0) throw new CryptographicException("Unexpected EOF while reading Ciphertext.");
                ctRead += read;
            }

            int tagRead = await inputStream.ReadAsync(tag, 0, tag.Length);
            if (tagRead != tag.Length) throw new CryptographicException("Unexpected EOF while reading Tag.");

            var plaintext = new byte[ciphertextLength];
            var aad = CreateAad(chunkIndex, ciphertextLength);

            try
            {
                aesGcm.Decrypt(nonce, ciphertext, tag, plaintext, aad);
            }
            catch (CryptographicException)
            {
                // Mask detailed exception to prevent timing/padding oracle type information leakage, 
                // though GCM is robust. Re-throw a standardized error.
                throw new CryptographicException($"Chunk {chunkIndex} failed authentication (tampered or corrupted).");
            }

            await outputStream.WriteAsync(plaintext, 0, plaintext.Length);
            chunkIndex++;
        }

        // Ensure no extra bytes at the end
        if (inputStream.Position != inputStream.Length)
        {
            throw new CryptographicException("Extra trailing bytes detected after valid chunks.");
        }
    }
}
