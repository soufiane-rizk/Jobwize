using JobWize.Shared.Contracts.Http.Attributes;

namespace JobWize.Modules.Applications.Contracts.Public.Companies;

public static class GetSelectableCompanies
{
    public const string Route = "/api/applications/companies";
    public sealed record Request([property: HttpQuery] string? Search);
    public sealed record Location(Guid Id, string Label);
    public sealed record Item(Guid Id, string Name, IReadOnlyList<Location> Locations);
    public sealed record Response(IReadOnlyList<Item> Companies);
}
