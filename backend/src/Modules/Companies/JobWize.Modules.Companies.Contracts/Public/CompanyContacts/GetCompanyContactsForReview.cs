namespace JobWize.Modules.Companies.Contracts.Public.CompanyContacts;

public static class GetCompanyContactsForReview
{
    public const string Route = "/api/admin/company-contacts/review";

    public sealed record Item(
        Guid Id,
        Guid CompanyId,
        string CompanyName,
        Guid? CompanyLocationId,
        string Name,
        string? RoleTitle,
        string? Email,
        string? PhoneNumber,
        DateTime CreatedAt);

    public sealed record Response(IReadOnlyList<Item> Contacts);
}
