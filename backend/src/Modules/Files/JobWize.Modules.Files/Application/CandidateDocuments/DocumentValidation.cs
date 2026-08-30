namespace JobWize.Modules.Files.Application.FileAssets;

internal static class DocumentValidation
{
    internal const long MaximumSizeBytes = 10 * 1024 * 1024;

    internal static bool IsSupported(string fileName, string? declaredContentType, ReadOnlySpan<byte> content, out string contentType)
    {
        string extension = Path.GetExtension(fileName).ToLowerInvariant();

        contentType = extension switch
        {
            ".pdf" when content.StartsWith("%PDF-"u8) => "application/pdf",
            ".doc" when content.Length >= 8 && content[..8].SequenceEqual(new byte[] { 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1 }) => "application/msword",
            ".docx" when IsZip(content) => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            _ => string.Empty
        };

        return contentType.Length > 0 &&
               (string.IsNullOrWhiteSpace(declaredContentType) ||
                string.Equals(declaredContentType, contentType, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(declaredContentType, "application/octet-stream", StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsZip(ReadOnlySpan<byte> content) =>
        content.Length >= 4 && content[..4].SequenceEqual(new byte[] { 0x50, 0x4B, 0x03, 0x04 });
}
