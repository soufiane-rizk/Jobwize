using JobWize.Shared.Contracts.Http.Attributes;

namespace JobWize.Modules.Applications.Contracts.Public.CompanyContacts;

public static class GetSelectableCompanyContacts
{
    public const string Route = "/api/applications/company-contacts";

    public sealed record Request(
        [property: HttpQuery] Guid? CompanyId,
        [property: HttpQuery] Guid? CompanyLocationId,
        [property: HttpQuery] string? Search);

    public sealed record Item(
        Guid Id,
        Guid CompanyId,
        Guid? CompanyLocationId,
        string Name,
        string? RoleTitle,
        string? Email,
        string? PhoneNumber);

    public sealed record Response(IReadOnlyList<Item> Contacts);
}
