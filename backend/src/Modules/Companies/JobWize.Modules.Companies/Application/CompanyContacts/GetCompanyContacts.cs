using JobWize.Modules.Companies.Contracts.Public.CompanyContacts;
using JobWize.Modules.Companies.Contracts.Public.Companies;
using JobWize.Modules.Companies.Persistence;
using JobWize.Runtime.Contracts.Dispatching;
using JobWize.Shared.Application.Results;
using JobWize.Shared.Application.Security;
using JobWize.Shared.Endpoints;
using JobWize.Shared.Runtime.Contracts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace JobWize.Modules.Companies.Application.CompanyContacts;

public static class GetCompanyContacts
{
    internal sealed record Query(Guid CompanyId)
        : IQuery<Contracts.Public.CompanyContacts.GetCompanyContacts.Response>;

    internal sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet(
                    Contracts.Public.CompanyContacts.GetCompanyContacts.Route,
                    async (
                        Guid companyId,
                        IDispatcher dispatcher,
                        CancellationToken cancellationToken) =>
                    {
                        Result<Contracts.Public.CompanyContacts.GetCompanyContacts.Response> result =
                            await dispatcher.SendAsync(new Query(companyId), cancellationToken);

                        return result.ToApiResult();
                    })
                .RequireAuthorization()
                .WithName("GetCompanyContacts")
                .WithTags("Company contacts");
        }
    }

    internal sealed class Handler(
        CompaniesDbContext dbContext,
        IUserContext userContext)
        : IQueryHandler<Query, Contracts.Public.CompanyContacts.GetCompanyContacts.Response>
    {
        public async Task<Result<Contracts.Public.CompanyContacts.GetCompanyContacts.Response>> HandleAsync(
            Query query,
            CancellationToken cancellationToken)
        {
            bool companyIsVisible = await dbContext.Companies.AnyAsync(
                company =>
                    company.Id == query.CompanyId &&
                    (company.Visibility == CompanyVisibility.Shared ||
                     company.CreatedByCandidateId == userContext.UserId),
                cancellationToken);

            if (!companyIsVisible)
            {
                return Result<Contracts.Public.CompanyContacts.GetCompanyContacts.Response>.Failure(
                    CompaniesErrors.CompanyNotFound);
            }

            List<Contracts.Public.CompanyContacts.GetCompanyContacts.Item> contacts = await dbContext.CompanyContacts
                .AsNoTracking()
                .Where(contact =>
                    contact.CompanyId == query.CompanyId &&
                    contact.IsActive &&
                    (contact.Visibility == CompanyContactVisibility.Shared ||
                     contact.CreatedByCandidateId == userContext.UserId))
                .OrderBy(contact => contact.Name)
                .Select(contact => new Contracts.Public.CompanyContacts.GetCompanyContacts.Item(
                    contact.Id,
                    contact.CompanyLocationId,
                    contact.Name,
                    contact.RoleTitle,
                    contact.Email,
                    contact.PhoneNumber,
                    contact.Visibility))
                .ToListAsync(cancellationToken);

            return Result<Contracts.Public.CompanyContacts.GetCompanyContacts.Response>.Success(
                new Contracts.Public.CompanyContacts.GetCompanyContacts.Response(contacts));
        }
    }
}
