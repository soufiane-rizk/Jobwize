using JobWize.Shared.Contracts.Http.Attributes;

namespace JobWize.Modules.Companies.Contracts.Public.CompanyContacts;

public static class CreateCompanyContact
{
    public const string Route = "/api/companies/{CompanyId}/contacts";

    public sealed record Request(
        [property: HttpRoute] Guid CompanyId,
        [property: HttpBody] Guid? CompanyLocationId,
        [property: HttpBody] string Name,
        [property: HttpBody] string? RoleTitle,
        [property: HttpBody] string? Email,
        [property: HttpBody] string? PhoneNumber);

    public sealed record Response(Guid Id);
}
