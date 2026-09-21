namespace TerryCorner.Application.Common.Interfaces;

public class FileValidationException : Exception
{
    public FileValidationException(string message) : base(message) { }
}

public record StoredFile(string StoredFileName, long SizeBytes);

/// <summary>
/// Handles secure storage of uploaded files (payment receipts). Implementations must
/// never trust the original client filename and must generate their own safe name.
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Saves the given stream under a server-generated safe filename and returns it.
    /// Throws <see cref="InvalidOperationException"/> if the extension/content type isn't allowed
    /// or the stream exceeds the configured size limit.
    /// </summary>
    Task<StoredFile> SaveReceiptAsync(Stream content, string originalFileName, string contentType, CancellationToken ct);

    /// <summary>Opens a previously stored receipt for reading. Callers must enforce access control themselves.</summary>
    Task<(Stream Content, string ContentType)> OpenReceiptAsync(string storedFileName, CancellationToken ct);

    /// <summary>Same validation/safe-naming guarantees as receipts, but images only and a smaller size limit.</summary>
    Task<StoredFile> SaveProfilePictureAsync(Stream content, string originalFileName, string contentType, CancellationToken ct);

    Task<(Stream Content, string ContentType)> OpenProfilePictureAsync(string storedFileName, CancellationToken ct);

    /// <summary>No-ops if the file is already gone — callers don't need to check existence first.</summary>
    void DeleteProfilePicture(string storedFileName);

    /// <summary>
    /// Stores a public, unauthenticated image (product/category photos) under the API's static
    /// file root and returns the relative URL path to serve it — e.g. "/uploads/images/xxx.jpg".
    /// Unlike receipts/profile pictures, no access control applies to these on purpose.
    /// </summary>
    Task<string> SavePublicImageAsync(Stream content, string originalFileName, string contentType, CancellationToken ct);
}
