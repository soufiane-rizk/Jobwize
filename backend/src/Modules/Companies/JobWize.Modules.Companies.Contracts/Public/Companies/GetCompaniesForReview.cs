namespace JobWize.Modules.Companies.Contracts.Public.Companies;

public static class GetCompaniesForReview
{
    public const string Route = "/api/admin/companies/review";

    public sealed record Item(
        Guid Id,
        string Name,
        string? Website,
        string? Industry,
        string? Description,
        Guid CandidateId,
        DateTime CreatedAt,
        IReadOnlyList<Location> Locations,
        IReadOnlyList<CompanyContact> Contacts);

    public sealed record Location(
        Guid Id,
        string? Label,
        string City,
        string Country,
        string? Address);

    public sealed record CompanyContact(
        Guid Id,
        Guid? CompanyLocationId,
        string Name,
        string? RoleTitle,
        string? Email,
        string? PhoneNumber);

    public sealed record Response(IReadOnlyList<Item> Companies);
}
