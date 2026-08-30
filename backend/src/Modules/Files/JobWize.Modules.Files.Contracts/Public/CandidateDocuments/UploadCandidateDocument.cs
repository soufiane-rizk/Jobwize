namespace JobWize.Modules.Files.Contracts.Public.FileAssets;

public static class UploadFileAsset
{
    public const string Route = "/api/files";

    public sealed record Response(Guid Id);
}
