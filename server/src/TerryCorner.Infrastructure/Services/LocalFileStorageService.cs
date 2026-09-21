using Microsoft.AspNetCore.Hosting;
using TerryCorner.Application.Common.Interfaces;

namespace TerryCorner.Infrastructure.Services;

public class LocalFileStorageService : IFileStorageService
{
    private static readonly HashSet<string> ReceiptExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".pdf",
    };

    private static readonly HashSet<string> ReceiptContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/png", "application/pdf",
    };

    private static readonly HashSet<string> ImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png",
    };

    private static readonly HashSet<string> ImageContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/png",
    };

    private const long MaxReceiptSizeBytes = 5 * 1024 * 1024; // 5 MB
    private const long MaxProfilePictureSizeBytes = 2 * 1024 * 1024; // 2 MB

    private static readonly HashSet<string> PublicImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp",
    };

    private static readonly HashSet<string> PublicImageContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/png", "image/webp",
    };

    private const long MaxPublicImageSizeBytes = 5 * 1024 * 1024; // 5 MB

    private readonly string _receiptsRoot;
    private readonly string _profilePicturesRoot;
    private readonly string _publicImagesRoot;

    public LocalFileStorageService(IWebHostEnvironment env)
    {
        // Stored outside wwwroot so files are never served by static-file middleware directly —
        // access must always go through an authorized controller action.
        _receiptsRoot = Path.Combine(env.ContentRootPath, "App_Data", "receipts");
        _profilePicturesRoot = Path.Combine(env.ContentRootPath, "App_Data", "profile-pictures");
        Directory.CreateDirectory(_receiptsRoot);
        Directory.CreateDirectory(_profilePicturesRoot);

        // Public images DO live under wwwroot on purpose — these need to be visible in a plain
        // <img> tag with no auth header, unlike receipts/profile pictures above.
        var webRoot = env.WebRootPath ?? Path.Combine(env.ContentRootPath, "wwwroot");
        _publicImagesRoot = Path.Combine(webRoot, "uploads", "images");
        Directory.CreateDirectory(_publicImagesRoot);
    }

    public Task<StoredFile> SaveReceiptAsync(Stream content, string originalFileName, string contentType, CancellationToken ct) =>
        SaveAsync(content, originalFileName, contentType, ct, _receiptsRoot,
            ReceiptExtensions, ReceiptContentTypes, MaxReceiptSizeBytes, "JPG, JPEG, PNG, PDF", "5 MB");

    public Task<(Stream Content, string ContentType)> OpenReceiptAsync(string storedFileName, CancellationToken ct) =>
        OpenAsync(storedFileName, _receiptsRoot, "receipt");

    public Task<StoredFile> SaveProfilePictureAsync(Stream content, string originalFileName, string contentType, CancellationToken ct) =>
        SaveAsync(content, originalFileName, contentType, ct, _profilePicturesRoot,
            ImageExtensions, ImageContentTypes, MaxProfilePictureSizeBytes, "JPG, JPEG, PNG", "2 MB");

    public Task<(Stream Content, string ContentType)> OpenProfilePictureAsync(string storedFileName, CancellationToken ct) =>
        OpenAsync(storedFileName, _profilePicturesRoot, "profile picture");

    public void DeleteProfilePicture(string storedFileName)
    {
        var fullPath = ResolveWithinRoot(storedFileName, _profilePicturesRoot);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
    }

    public async Task<string> SavePublicImageAsync(Stream content, string originalFileName, string contentType, CancellationToken ct)
    {
        var stored = await SaveAsync(content, originalFileName, contentType, ct, _publicImagesRoot,
            PublicImageExtensions, PublicImageContentTypes, MaxPublicImageSizeBytes, "JPG, JPEG, PNG, WEBP", "5 MB");

        return $"/uploads/images/{stored.StoredFileName}";
    }

    private static async Task<StoredFile> SaveAsync(
        Stream content, string originalFileName, string contentType, CancellationToken ct,
        string storageRoot, HashSet<string> allowedExtensions, HashSet<string> allowedContentTypes,
        long maxSizeBytes, string allowedDescription, string maxSizeDescription)
    {
        // Never trust the client's filename or declared content type for the extension —
        // derive it ourselves from an allow-list.
        var extension = Path.GetExtension(originalFileName);
        if (string.IsNullOrEmpty(extension) || !allowedExtensions.Contains(extension))
        {
            throw new FileValidationException($"Unsupported file type. Allowed: {allowedDescription}.");
        }

        if (!allowedContentTypes.Contains(contentType))
        {
            throw new FileValidationException($"Unsupported file type. Allowed: {allowedDescription}.");
        }

        if (content.Length > maxSizeBytes)
        {
            throw new FileValidationException($"File is too large. Maximum size is {maxSizeDescription}.");
        }

        // Server-generated name only — prevents path traversal and filename collisions entirely.
        var safeFileName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
        var fullPath = Path.Combine(storageRoot, safeFileName);

        await using (var fileStream = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write))
        {
            await content.CopyToAsync(fileStream, ct);
        }

        return new StoredFile(safeFileName, content.Length);
    }

    private static Task<(Stream Content, string ContentType)> OpenAsync(
        string storedFileName, string storageRoot, string kindForErrorMessage)
    {
        var fullPath = ResolveWithinRoot(storedFileName, storageRoot);

        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException($"The requested {kindForErrorMessage} could not be found.");
        }

        var extension = Path.GetExtension(fullPath).ToLowerInvariant();
        var contentType = extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".pdf" => "application/pdf",
            _ => "application/octet-stream",
        };

        Stream stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read);
        return Task.FromResult((stream, contentType));
    }

    private static string ResolveWithinRoot(string storedFileName, string storageRoot)
    {
        // storedFileName is always our own GUID-based name (never client input at this point),
        // but we still resolve and verify it stays within the storage root as defense in depth.
        var fullPath = Path.GetFullPath(Path.Combine(storageRoot, storedFileName));
        if (!fullPath.StartsWith(storageRoot, StringComparison.Ordinal))
        {
            throw new FileValidationException("Invalid file reference.");
        }

        return fullPath;
    }
}
