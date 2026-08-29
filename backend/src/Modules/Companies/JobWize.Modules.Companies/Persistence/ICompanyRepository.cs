using JobWize.Modules.Companies.Domain;
using Microsoft.EntityFrameworkCore;

namespace JobWize.Modules.Companies.Persistence;

public interface ICompanyRepository
{
    Task<Company?> GetByIdAsync(Guid companyId, CancellationToken cancellationToken = default);
    Task SaveAsync(Company company, CancellationToken cancellationToken = default);
}

internal sealed class CompanyRepository(CompaniesDbContext dbContext) : ICompanyRepository
{
    public Task<Company?> GetByIdAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        return dbContext.Companies
            .Include(company => company.Locations)
            .Include(company => company.Contacts)
            .SingleOrDefaultAsync(company => company.Id == companyId, cancellationToken);
    }

    public Task SaveAsync(Company company, CancellationToken cancellationToken = default)
    {
        if (dbContext.Entry(company).State == EntityState.Detached)
        {
            dbContext.Companies.Add(company);
        }
        return Task.CompletedTask;
    }
}
