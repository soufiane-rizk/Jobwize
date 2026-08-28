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

namespace JobWize.Modules.Companies.Application.Companies;

public static class GetCompany
{
    internal sealed record Query(Guid Id) : IQuery<Contracts.Public.Companies.GetCompany.Response>;

    internal sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet(
                    Contracts.Public.Companies.GetCompany.Route,
                    async (
                        Guid id,
                        IDispatcher dispatcher,
                        CancellationToken cancellationToken) =>
                    {
                        Result<Contracts.Public.Companies.GetCompany.Response> result =
                            await dispatcher.SendAsync(
                                new Query(id),
                                cancellationToken);

                        return result.ToApiResult();
                    })
                .RequireAuthorization()
                .WithName("GetCompany")
                .WithTags("Companies");
        }
    }

    internal sealed class Handler(
        CompaniesDbContext dbContext,
        IUserContext userContext) : IQueryHandler<Query, Contracts.Public.Companies.GetCompany.Response>
    {
        public async Task<Result<Contracts.Public.Companies.GetCompany.Response>> HandleAsync(
            Query query,
            CancellationToken cancellationToken)
        {
            Contracts.Public.Companies.GetCompany.Response? company = await dbContext.Companies
                .AsNoTracking()
                .Where(item =>
                    item.Id == query.Id &&
                    (item.Visibility == CompanyVisibility.Shared ||
                     item.CreatedByCandidateId == userContext.UserId))
                .Select(item => new Contracts.Public.Companies.GetCompany.Response(
                    item.Id,
                    item.Name,
                    item.Website,
                    item.Industry,
                    item.Description,
                    item.Visibility,
                    item.Locations
                        .OrderBy(location => location.Label)
                        .Select(location => new Contracts.Public.Companies.GetCompanies.Location(
                            location.Id,
                            location.Label,
                            location.City,
                            location.Country,
                            location.Address))
                        .ToList()))
                .SingleOrDefaultAsync(cancellationToken);

            if (company is null)
            {
                return Result<Contracts.Public.Companies.GetCompany.Response>.Failure(
                    CompaniesErrors.CompanyNotFound);
            }

            return Result<Contracts.Public.Companies.GetCompany.Response>.Success(company);
        }
    }
}
