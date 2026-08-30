namespace JobWize.Modules.Companies.Contracts.Public.Companies;

public static class GetCompaniesForReview
{
    public const string Route = "/api/admin/companies/review";

    public sealed record Item(
        Guid Id,
        string Name,
        string? Website,
        string? Industry,
        string? Description,
        Guid CandidateId,
        DateTime CreatedAt,
        IReadOnlyList<GetCompanies.Location> Locations);

    public sealed record Response(IReadOnlyList<Item> Companies);
}
