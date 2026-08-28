using JobWize.Modules.Companies.Persistence;
using JobWize.Runtime.Contracts.Requests;
using Microsoft.EntityFrameworkCore;

namespace JobWize.Modules.Companies.Application.Companies;

internal sealed class GetAllCompanyProjectionsHandler(CompaniesDbContext dbContext)
    : IModuleQueryHandler<Contracts.Internal.Companies.GetAllCompanyProjections.Query, IReadOnlyList<Contracts.Internal.Companies.GetCompanyProjection.Response>>
{
    public async Task<IReadOnlyList<Contracts.Internal.Companies.GetCompanyProjection.Response>> HandleAsync(
        Contracts.Internal.Companies.GetAllCompanyProjections.Query query,
        CancellationToken cancellationToken)
    {
        return await dbContext.Companies
            .AsNoTracking()
            .Include(company => company.Locations)
            .Select(company => new Contracts.Internal.Companies.GetCompanyProjection.Response(
                company.Id,
                company.Name,
                company.Visibility,
                company.CreatedByCandidateId,
                company.Locations
                    .OrderBy(location => location.Label)
                    .Select(location => new Contracts.Internal.Companies.GetCompanyProjection.Location(
                        location.Id,
                        location.CompanyId,
                        location.Label))
                    .ToList()))
            .ToListAsync(cancellationToken);
    }
}
