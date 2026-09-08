using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Upload;
using Microsoft.Extensions.Configuration;
using SmartPharmacySystem.Application.IServices;

namespace SmartPharmacySystem.Infrastructure.Services;

public class GoogleDriveService : IGoogleDriveService
{
    private readonly IConfiguration _configuration;
    private readonly string _applicationName = "SmartPharmacySystem Backup";

    public GoogleDriveService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    private DriveService CreateDriveService()
    {
        var credentialsPath = _configuration["BackupSettings:GoogleServiceAccountJsonPath"];
        if (string.IsNullOrWhiteSpace(credentialsPath) || !File.Exists(credentialsPath))
        {
            throw new InvalidOperationException("Google Service Account JSON file path is not configured or the file does not exist.");
        }

        GoogleCredential credential;
        using (var stream = new FileStream(credentialsPath, FileMode.Open, FileAccess.Read))
        {
            credential = GoogleCredential.FromStream(stream)
                .CreateScoped(DriveService.Scope.DriveFile);
        }

        return new DriveService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = _applicationName
        });
    }

    public async Task<(bool IsSuccess, string Message)> TestConnectionAsync()
    {
        try
        {
            using var service = CreateDriveService();
            // Just request a list of files to test authentication
            var request = service.Files.List();
            request.PageSize = 1;
            request.Fields = "files(id, name)";
            
            await request.ExecuteAsync();
            return (true, "تم الاتصال بخدمة Google Drive بنجاح.");
        }
        catch (Exception ex)
        {
            return (false, $"فشل الاتصال بـ Google Drive: {ex.Message}");
        }
    }

    public async Task<string> UploadFileAsync(string localFilePath, string fileName, string folderId)
    {
        if (string.IsNullOrWhiteSpace(folderId))
        {
            throw new ArgumentException("Folder ID must be provided.");
        }

        using var service = CreateDriveService();

        var fileMetadata = new Google.Apis.Drive.v3.Data.File()
        {
            Name = fileName,
            Parents = new[] { folderId }
        };

        using var stream = new FileStream(localFilePath, FileMode.Open, FileAccess.Read);

        var request = service.Files.Create(fileMetadata, stream, "application/octet-stream");
        request.Fields = "id, size, parents";

        using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(3));
        
        // Use resumable upload for large backup files
        var progress = await request.UploadAsync(cts.Token);

        if (progress.Status == Google.Apis.Upload.UploadStatus.Failed)
        {
            throw new Exception($"فشل رفع الملف: {progress.Exception?.Message}", progress.Exception);
        }

        var uploadedFile = request.ResponseBody;
        if (uploadedFile == null || string.IsNullOrEmpty(uploadedFile.Id))
        {
            throw new Exception("الرفع اكتمل ولكن لم يتم إرجاع معرف الملف من Google Drive.");
        }

        return uploadedFile.Id;
    }

    public async Task<bool> VerifyUploadAsync(string fileId, long expectedSizeBytes, string expectedFolderId)
    {
        using var service = CreateDriveService();

        try
        {
            var request = service.Files.Get(fileId);
            request.Fields = "id, size, parents";
            
            using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(1));
            var file = await request.ExecuteAsync(cts.Token);

            if (file == null) return false;

            if (file.Size != expectedSizeBytes) return false;

            if (file.Parents == null || !file.Parents.Contains(expectedFolderId)) return false;

            return true;
        }
        catch
        {
            return false; // Any API error implies verification failed
        }
    }

    public async Task DeleteFileAsync(string fileId)
    {
        using var service = CreateDriveService();
        try
        {
            var request = service.Files.Delete(fileId);
            await request.ExecuteAsync();
        }
        catch
        {
            // Ignore deletion failures so retention policy loop can continue
        }
    }
}
