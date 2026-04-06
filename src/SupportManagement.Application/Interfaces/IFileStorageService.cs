namespace SupportManagement.Application.Interfaces;

public interface IFileStorageService
{
    Task<(string StoredFileName, string FilePath)> SaveFileAsync(Stream fileStream, string originalFileName, string folder, CancellationToken cancellationToken = default);
    Task DeleteFileAsync(string filePath, CancellationToken cancellationToken = default);
    string GetFileUrl(string filePath);
}
