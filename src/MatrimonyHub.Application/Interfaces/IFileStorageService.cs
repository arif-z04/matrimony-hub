using Microsoft.AspNetCore.Http;

namespace MatrimonyHub.Application.Interfaces;

public interface IFileStorageService
{
    Task<string> SaveFileAsync(IFormFile file, string subFolder);
    void DeleteFile(string relativePath);
    bool IsValidImage(IFormFile file);
    bool IsValidDocument(IFormFile file);
}
