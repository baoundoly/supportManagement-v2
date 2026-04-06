using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using SupportManagement.Application.Interfaces;
using SupportManagement.Domain.Constants;

namespace SupportManagement.Infrastructure.Storage;

public class LocalFileStorageService : IFileStorageService
{
    private readonly string _uploadPath;
    private readonly string _baseUrl;

    public LocalFileStorageService(IHostEnvironment env, IConfiguration configuration)
    {
        _uploadPath = Path.Combine(env.ContentRootPath, "uploads");
        _baseUrl = configuration["Storage:BaseUrl"] ?? "/uploads";
        Directory.CreateDirectory(_uploadPath);
    }

    public async Task<(string StoredFileName, string FilePath)> SaveFileAsync(Stream fileStream, string originalFileName, string folder, CancellationToken cancellationToken = default)
    {
        if (fileStream.Length > AppConstants.MaxAttachmentSizeBytes)
            throw new InvalidOperationException($"File size exceeds maximum allowed size of {AppConstants.MaxAttachmentSizeMb}MB.");

        var extension = Path.GetExtension(originalFileName).ToLowerInvariant();
        if (!AppConstants.AllowedAttachmentTypes.Contains(extension))
            throw new InvalidOperationException($"File type '{extension}' is not allowed.");

        var folderPath = Path.Combine(_uploadPath, folder);
        Directory.CreateDirectory(folderPath);

        var storedFileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(folder, storedFileName);
        var fullPath = Path.Combine(_uploadPath, filePath);

        await using var fileOutputStream = File.Create(fullPath);
        await fileStream.CopyToAsync(fileOutputStream, cancellationToken);

        return (storedFileName, filePath);
    }

    public Task DeleteFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_uploadPath, filePath);
        if (File.Exists(fullPath))
            File.Delete(fullPath);
        return Task.CompletedTask;
    }

    public string GetFileUrl(string filePath) => $"{_baseUrl}/{filePath.Replace(Path.DirectorySeparatorChar, '/')}";
}
