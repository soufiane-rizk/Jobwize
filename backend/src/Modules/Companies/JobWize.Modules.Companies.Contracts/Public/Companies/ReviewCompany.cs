using JobWize.Shared.Contracts.Http.Attributes;

namespace JobWize.Modules.Companies.Contracts.Public.Companies;

public static class ReviewCompany
{
    public const string Route = "/api/companies/{Id}/review";

    public sealed record Request(
        [property: HttpRoute] Guid Id,
        [property: HttpBody] bool Approved,
        [property: HttpBody] string? Reason,
        [property: HttpBody] string? Name,
        [property: HttpBody] string? Website,
        [property: HttpBody] string? Industry,
        [property: HttpBody] string? Description);
}
