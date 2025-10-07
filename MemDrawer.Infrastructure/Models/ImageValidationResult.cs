namespace MemDrawer.Infrastructure.Models;

/// <summary>
/// Result of image validation process
/// </summary>
/// <param name="IsValid">Indicates if the image is valid</param>
/// <param name="ErrorMessage">Error message if the image is not valid</param>
/// <param name="ConvertedStream">Stream containing the converted image if applicable</param>
public record ImageValidationResult(bool IsValid, string? ErrorMessage = null, Stream? ConvertedStream = null);