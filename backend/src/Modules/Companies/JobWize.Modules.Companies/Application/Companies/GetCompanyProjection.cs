using JobWize.Modules.Companies.Persistence;
using JobWize.Runtime.Contracts.Requests;
using Microsoft.EntityFrameworkCore;

namespace JobWize.Modules.Companies.Application.Companies;

internal sealed class GetCompanyProjectionHandler(CompaniesDbContext dbContext)
    : IModuleQueryHandler<Contracts.Internal.Companies.GetCompanyProjection.Query, Contracts.Internal.Companies.GetCompanyProjection.Response>
{
    public async Task<Contracts.Internal.Companies.GetCompanyProjection.Response> HandleAsync(
        Contracts.Internal.Companies.GetCompanyProjection.Query query,
        CancellationToken cancellationToken)
    {
        Domain.Company? company = await dbContext.Companies.FindAsync(
            [query.CompanyId],
            cancellationToken);

        if (company is null || dbContext.Entry(company).State != EntityState.Added)
        {
            company = await dbContext.Companies
                .AsNoTracking()
                .Include(item => item.Locations)
                .SingleAsync(item => item.Id == query.CompanyId, cancellationToken);
        }

        return new(
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
                .ToList());
    }
}
