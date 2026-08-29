using JobWize.Shared.Contracts.Http.Attributes;

namespace JobWize.Modules.Companies.Contracts.Public.CompanyContacts;

public static class ReviewCompanyContact
{
    public const string Route = "/api/company-contacts/{Id}/review";

    public sealed record Request(
        [property: HttpRoute] Guid Id,
        [property: HttpBody] bool Approved,
        [property: HttpBody] string? Reason,
        [property: HttpBody] Guid? CompanyLocationId,
        [property: HttpBody] string? Name,
        [property: HttpBody] string? RoleTitle,
        [property: HttpBody] string? Email,
        [property: HttpBody] string? PhoneNumber);
}
