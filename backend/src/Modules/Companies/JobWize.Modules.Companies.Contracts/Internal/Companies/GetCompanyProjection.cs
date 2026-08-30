using JobWize.Runtime.Contracts.Requests;

namespace JobWize.Modules.Companies.Contracts.Internal.Companies;

public static class GetCompanyProjection
{
    public sealed record Query(Guid CompanyId) : IModuleQuery<Response>;

    public sealed record Location(Guid Id, Guid CompanyId, string Label);

    public sealed record Response(Guid Id, string Name, Contracts.Public.Companies.CompanyVisibility Visibility, Guid? CreatedByCandidateId, IReadOnlyList<Location> Locations);
}
