using JobWize.Modules.Companies.Contracts.Public.CompanyContacts;
using JobWize.Modules.Companies.Contracts.Public.Companies;
using JobWize.Modules.Companies.Persistence;
using JobWize.Runtime.Contracts.Dispatching;
using JobWize.Shared.Application.Results;
using JobWize.Shared.Endpoints;
using JobWize.Shared.Runtime.Contracts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace JobWize.Modules.Companies.Application.CompanyContacts;

public static class GetCompanyContactsForReview
{
    internal sealed record Query
        : IQuery<Contracts.Public.CompanyContacts.GetCompanyContactsForReview.Response>;

    internal sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet(
                    Contracts.Public.CompanyContacts.GetCompanyContactsForReview.Route,
                    async (IDispatcher dispatcher, CancellationToken cancellationToken) =>
                    {
                        Result<Contracts.Public.CompanyContacts.GetCompanyContactsForReview.Response> result =
                            await dispatcher.SendAsync(new Query(), cancellationToken);

                        return result.ToApiResult();
                    })
                .RequireAuthorization(global::JobWize.Modules.Identity.Contracts.Public.Authentication.AuthenticationPolicies.UserManagement)
                .WithName("GetCompanyContactsForReview")
                .WithTags("Company contacts");
        }
    }

    internal sealed class Handler(CompaniesDbContext dbContext)
        : IQueryHandler<Query, Contracts.Public.CompanyContacts.GetCompanyContactsForReview.Response>
    {
        public async Task<Result<Contracts.Public.CompanyContacts.GetCompanyContactsForReview.Response>> HandleAsync(
            Query query,
            CancellationToken cancellationToken)
        {
            List<Contracts.Public.CompanyContacts.GetCompanyContactsForReview.Item> contacts =
                await dbContext.CompanyContacts
                    .AsNoTracking()
                        .Where(contact =>
                            contact.Visibility == CompanyContactVisibility.Private &&
                            contact.CreatedByCandidateId != null &&
                            contact.ReviewedAt == null)
                    .Join(
                        dbContext.Companies.AsNoTracking(),
                        contact => contact.CompanyId,
                        company => company.Id,
                        (contact, company) => new
                        {
                            Contact = contact,
                            CompanyName = company.Name,
                            CompanyVisibility = company.Visibility
                        })
                    .Where(item => item.CompanyVisibility == CompanyVisibility.Shared)
                    .OrderBy(item => item.Contact.CreatedAt)
                    .Select(item => new Contracts.Public.CompanyContacts.GetCompanyContactsForReview.Item(
                        item.Contact.Id,
                        item.Contact.CompanyId,
                        item.CompanyName,
                        item.Contact.CompanyLocationId,
                        item.Contact.Name,
                        item.Contact.RoleTitle,
                        item.Contact.Email,
                        item.Contact.PhoneNumber,
                        item.Contact.CreatedAt))
                    .ToListAsync(cancellationToken);

            return Result<Contracts.Public.CompanyContacts.GetCompanyContactsForReview.Response>.Success(
                new Contracts.Public.CompanyContacts.GetCompanyContactsForReview.Response(contacts));
        }
    }
}
