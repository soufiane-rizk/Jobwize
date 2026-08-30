using JobWize.Shared.Contracts.Http.Attributes;

namespace JobWize.Modules.Companies.Contracts.Public.Companies;

public static class CreatePrivateCompany
{
    public const string Route = "/api/companies";

    public sealed record Location(
        string Label,
        string City,
        string Country,
        string? Address);

    public sealed record Request(
        [property: HttpBody] string Name,
        [property: HttpBody] string? Website,
        [property: HttpBody] string? Industry,
        [property: HttpBody] string? Description,
        [property: HttpBody] IReadOnlyList<Location> Locations);

    public sealed record Response(Guid Id);
}
