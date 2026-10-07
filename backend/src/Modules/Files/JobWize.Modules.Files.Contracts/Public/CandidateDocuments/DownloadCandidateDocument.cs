using JobWize.Shared.Contracts.Http.Attributes;

namespace JobWize.Modules.Files.Contracts.Public.FileAssets;

public static class DownloadFileAsset
{
    public const string Route = "/api/files/{DocumentId}/download";

    public sealed record Request([property: HttpRoute] Guid DocumentId);
}
