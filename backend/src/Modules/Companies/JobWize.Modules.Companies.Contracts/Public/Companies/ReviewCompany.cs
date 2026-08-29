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
        [property: HttpBody] string? Description,
        [property: HttpBody] IReadOnlyList<Location>? Locations = null,
        [property: HttpBody] IReadOnlyList<Contact>? Contacts = null);

    public sealed record Location(
        Guid? Id,
        bool Approved,
        string? Reason,
        string? Label,
        string City,
        string Country,
        string? Address);

    public sealed record Contact(
        Guid? Id,
        bool Approved,
        string? Reason,
        int? LocationIndex,
        string Name,
        string? RoleTitle,
        string? Email,
        string? PhoneNumber);
}
