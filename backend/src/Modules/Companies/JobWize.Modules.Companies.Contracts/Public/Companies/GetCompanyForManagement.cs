using JobWize.Modules.Companies.Contracts.Public.CompanyContacts;
using JobWize.Shared.Contracts.Http.Attributes;

namespace JobWize.Modules.Companies.Contracts.Public.Companies;

public static class GetCompanyForManagement
{
    public const string Route = "/api/admin/companies/{Id}/management";

    public sealed record Request([property: HttpRoute] Guid Id);

    public sealed record Location(
        Guid Id,
        string? Label,
        string City,
        string Country,
        string? Address,
        CompanyLocationVisibility Visibility,
        bool IsActive,
        Guid? CreatedByCandidateId,
        DateTime? ReviewedAt,
        string? ReviewReason);

    public sealed record Contact(
        Guid Id,
        Guid? CompanyLocationId,
        string Name,
        string? RoleTitle,
        string? Email,
        string? PhoneNumber,
        CompanyContactVisibility Visibility,
        bool IsActive,
        Guid? CreatedByCandidateId,
        DateTime? ReviewedAt,
        string? ReviewReason);

    public sealed record Response(
        Guid Id,
        string Name,
        string? Website,
        string? Industry,
        string? Description,
        CompanyVisibility Visibility,
        IReadOnlyList<Location> Locations,
        IReadOnlyList<Contact> Contacts);
}
