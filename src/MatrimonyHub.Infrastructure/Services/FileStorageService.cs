using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MatrimonyHub.Application.Interfaces;

namespace MatrimonyHub.Infrastructure.Services;

public class FileStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<FileStorageService> _logger;

    private readonly string[] _allowedImageExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
    private readonly string[] _allowedDocExtensions = { ".jpg", ".jpeg", ".png", ".pdf" };

    public FileStorageService(IWebHostEnvironment env, ILogger<FileStorageService> logger)
    {
        _env = env;
        _logger = logger;
    }

    public async Task<string> SaveFileAsync(IFormFile file, string subFolder)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("Invalid file");

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var secureFileName = $"{Guid.NewGuid():N}{extension}";

        var uploadsRoot = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads", subFolder);
        if (!Directory.Exists(uploadsRoot))
        {
            Directory.CreateDirectory(uploadsRoot);
        }

        var destinationPath = Path.Combine(uploadsRoot, secureFileName);
        using (var stream = new FileStream(destinationPath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        _logger.LogInformation("File saved securely to uploads/{SubFolder}/{FileName}", subFolder, secureFileName);
        return $"/uploads/{subFolder}/{secureFileName}";
    }

    public void DeleteFile(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath)) return;

        try
        {
            var trimmed = relativePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var fullPath = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), trimmed);
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                _logger.LogInformation("File deleted: {Path}", fullPath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete file: {Path}", relativePath);
        }
    }

    public bool IsValidImage(IFormFile file)
    {
        if (file == null || file.Length == 0) return false;
        if (file.Length > 5 * 1024 * 1024) return false; // 5 MB max

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        return _allowedImageExtensions.Contains(ext);
    }

    public bool IsValidDocument(IFormFile file)
    {
        if (file == null || file.Length == 0) return false;
        if (file.Length > 10 * 1024 * 1024) return false; // 10 MB max

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        return _allowedDocExtensions.Contains(ext);
    }
}
