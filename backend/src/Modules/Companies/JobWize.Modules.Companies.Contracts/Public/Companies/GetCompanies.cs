using JobWize.Shared.Contracts.Http.Attributes;

namespace JobWize.Modules.Companies.Contracts.Public.Companies;

public static class GetCompanies
{
    public const string Route = "/api/companies";

    public sealed record Request([property: HttpQuery] string? Search);

    public sealed record Location(
        Guid Id,
        string Label,
        string City,
        string Country,
        string? Address);

    public sealed record Item(
        Guid Id,
        string Name,
        string? Website,
        string? Industry,
        string? Description,
        CompanyVisibility Visibility,
        IReadOnlyList<Location> Locations);

    public sealed record Response(IReadOnlyList<Item> Companies);
}
