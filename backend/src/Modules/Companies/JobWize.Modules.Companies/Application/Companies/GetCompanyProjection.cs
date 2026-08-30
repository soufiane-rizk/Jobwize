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

        if (company is not null)
        {
            dbContext.ChangeTracker.DetectChanges();

            EntityState state = dbContext.Entry(company).State;

            if (state == EntityState.Added)
            {
                return CreateResponse(company);
            }

            if (state == EntityState.Modified)
            {
                await dbContext.Entry(company)
                    .Collection(item => item.Locations)
                    .LoadAsync(cancellationToken);
                await dbContext.Entry(company)
                    .Collection(item => item.Contacts)
                    .LoadAsync(cancellationToken);

                return CreateResponse(company);
            }
        }

        company = await dbContext.Companies
            .AsNoTracking()
            .Include(item => item.Locations)
            .Include(item => item.Contacts)
            .SingleAsync(item => item.Id == query.CompanyId, cancellationToken);

        return CreateResponse(company);
    }

    private static Contracts.Internal.Companies.GetCompanyProjection.Response CreateResponse(
        Domain.Company company)
    {
        return new(
            company.Id,
            company.Name,
            company.Visibility,
            company.CreatedByCandidateId,
            company.Locations
                .OrderBy(location => location.City)
                .ThenBy(location => location.Country)
                .ThenBy(location => location.Label)
                .Select(location => new Contracts.Internal.Companies.GetCompanyProjection.Location(
                    location.Id,
                    location.CompanyId,
                    location.Label ?? (location.City + ", " + location.Country),
                    location.Visibility,
                    location.CreatedByCandidateId,
                    location.IsActive))
                .ToList(),
            company.Contacts
                .OrderBy(contact => contact.Name)
                .Select(contact => new Contracts.Internal.Companies.GetCompanyProjection.Contact(
                    contact.Id,
                    contact.CompanyId,
                    contact.CompanyLocationId,
                    contact.Name,
                    contact.RoleTitle,
                    contact.Email,
                    contact.PhoneNumber,
                    contact.Visibility,
                    contact.CreatedByCandidateId,
                    contact.IsActive,
                    contact.Visibility == Contracts.Public.CompanyContacts.CompanyContactVisibility.Private &&
                    contact.ReviewedAt is not null))
                .ToList());
    }
}
