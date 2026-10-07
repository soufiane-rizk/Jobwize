using JobWize.Runtime.Contracts.Requests;

namespace JobWize.Modules.Companies.Contracts.Internal.Companies;

public static class GetAllCompanyProjections
{
    public sealed record Query : IModuleQuery<IReadOnlyList<GetCompanyProjection.Response>>;
}
