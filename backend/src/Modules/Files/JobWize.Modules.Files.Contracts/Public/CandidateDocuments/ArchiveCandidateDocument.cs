using JobWize.Shared.Contracts.Http.Attributes;

namespace JobWize.Modules.Files.Contracts.Public.FileAssets;

public static class ArchiveFileAsset
{
    public const string Route = "/api/files/{DocumentId}";

    public sealed record Request([property: HttpRoute] Guid DocumentId);
}
