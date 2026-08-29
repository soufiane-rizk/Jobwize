using JobWize.Shared.Contracts.Http.Attributes;

namespace JobWize.Modules.Companies.Contracts.Public.CompanyContacts;

public static class GetCompanyContacts
{
    public const string Route = "/api/companies/{CompanyId}/contacts";

    public sealed record Request([property: HttpRoute] Guid CompanyId);

    public sealed record Item(
        Guid Id,
        Guid? CompanyLocationId,
        string Name,
        string? RoleTitle,
        string? Email,
        string? PhoneNumber,
        CompanyContactVisibility Visibility);

    public sealed record Response(IReadOnlyList<Item> Contacts);
}
