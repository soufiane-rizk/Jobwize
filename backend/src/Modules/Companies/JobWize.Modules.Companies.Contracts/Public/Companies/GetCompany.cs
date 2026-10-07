using JobWize.Shared.Contracts.Http.Attributes;

namespace JobWize.Modules.Companies.Contracts.Public.Companies;

public static class GetCompany
{
    public const string Route = "/api/companies/{Id}";

    public sealed record Request([property: HttpRoute] Guid Id);

    public sealed record Response(
        Guid Id,
        string Name,
        string? Website,
        string? Industry,
        string? Description,
        CompanyVisibility Visibility,
        IReadOnlyList<GetCompanies.Location> Locations);
}
