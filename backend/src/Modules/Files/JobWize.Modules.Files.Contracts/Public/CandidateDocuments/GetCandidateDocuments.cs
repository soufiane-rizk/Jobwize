namespace JobWize.Modules.Files.Contracts.Public.FileAssets;

public static class GetFileAssets
{
    public const string Route = "/api/files";

    public sealed record Item(
        Guid Id,
        string FileName,
        string ContentType,
        long SizeBytes,
        DateTime UploadedAt);

    public sealed record Response(IReadOnlyList<Item> Files);
}
