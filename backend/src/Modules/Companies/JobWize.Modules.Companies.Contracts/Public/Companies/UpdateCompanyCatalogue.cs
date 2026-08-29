using JobWize.Shared.Contracts.Http.Attributes;

namespace JobWize.Modules.Companies.Contracts.Public.Companies;

public static class UpdateCompanyCatalogue
{
    public const string Route = "/api/admin/companies/{Id}/management";

    public sealed record Location(
        Guid? Id,
        string? Label,
        string City,
        string Country,
        string? Address,
        bool IsActive);

    public sealed record Contact(
        Guid? Id,
        int? LocationIndex,
        string Name,
        string? RoleTitle,
        string? Email,
        string? PhoneNumber,
        bool IsActive);

    public sealed record Request(
        [property: HttpRoute] Guid Id,
        [property: HttpBody] string Name,
        [property: HttpBody] string? Website,
        [property: HttpBody] string? Industry,
        [property: HttpBody] string? Description,
        [property: HttpBody] IReadOnlyList<Location> Locations,
        [property: HttpBody] IReadOnlyList<Contact> Contacts);
}
