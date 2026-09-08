using System.Threading.Tasks;

namespace SmartPharmacySystem.Application.IServices;

public interface IGoogleDriveService
{
    /// <summary>
    /// Tests the connection to Google Drive using the configured service account.
    /// Returns true if successful, false otherwise.
    /// </summary>
    Task<(bool IsSuccess, string Message)> TestConnectionAsync();

    /// <summary>
    /// Uploads a file to Google Drive using Resumable Upload.
    /// Returns the Google Drive File ID on success.
    /// </summary>
    Task<string> UploadFileAsync(string localFilePath, string fileName, string folderId);

    /// <summary>
    /// Verifies that a file uploaded to Google Drive exists, its size matches the local file,
    /// and it is located in the correct folder.
    /// </summary>
    Task<bool> VerifyUploadAsync(string fileId, long expectedSizeBytes, string expectedFolderId);

    /// <summary>
    /// Deletes a file from Google Drive. Used for retention policies.
    /// </summary>
    Task DeleteFileAsync(string fileId);
}
