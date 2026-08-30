using JobWize.Runtime.Contracts.Requests;

namespace JobWize.Modules.Files.Contracts.Internal.FileAssets;

public static class GetCandidateDocumentsForSubmission
{
    public sealed record Query(Guid CandidateId, IReadOnlyList<Guid> FileIds) : IModuleQuery<Response>;

    public sealed record Item(Guid FileId, string FileName, string ContentType, long SizeBytes);

    public sealed record Response(IReadOnlyList<Item> Files);
}
