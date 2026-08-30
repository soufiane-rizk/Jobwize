using FluentValidation;
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

public static class ReviewCompanyContact
{
    internal sealed record Command(
        Guid Id,
        bool Approved,
        string? Reason,
        Guid? CompanyLocationId,
        string? Name,
        string? RoleTitle,
        string? Email,
        string? PhoneNumber) : ICommand<bool>;

    internal sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(command => command.Reason)
                .NotEmpty()
                .MaximumLength(4000)
                .When(command => !command.Approved);
            RuleFor(command => command.Name).NotEmpty().MaximumLength(200).When(command => command.Approved);
            RuleFor(command => command.RoleTitle).MaximumLength(200);
            RuleFor(command => command.Email).MaximumLength(320).EmailAddress();
            RuleFor(command => command.PhoneNumber).MaximumLength(50);
        }
    }

    internal sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost(
                    Contracts.Public.CompanyContacts.ReviewCompanyContact.Route,
                    async (
                        Guid id,
                        Contracts.Public.CompanyContacts.ReviewCompanyContact.Request request,
                        IDispatcher dispatcher,
                        CancellationToken cancellationToken) =>
                    {
                        Result<bool> result = await dispatcher.SendAsync(
                            new Command(
                                id,
                                request.Approved,
                                request.Reason,
                                request.CompanyLocationId,
                                request.Name,
                                request.RoleTitle,
                                request.Email,
                                request.PhoneNumber),
                            cancellationToken);

                        return result.ToApiResult();
                    })
                .RequireAuthorization(global::JobWize.Modules.Identity.Contracts.Public.Authentication.AuthenticationPolicies.UserManagement)
                .WithName("ReviewCompanyContact")
                .WithTags("Company contacts");
        }
    }

    internal sealed class Handler(
        CompaniesDbContext dbContext,
        ICompanyRepository companies,
        IUserContext userContext) : ICommandHandler<Command, bool>
    {
        public async Task<Result<bool>> HandleAsync(Command command, CancellationToken cancellationToken)
        {
            Guid? companyId = await dbContext.CompanyContacts
                .Where(contact => contact.Id == command.Id)
                .Select(contact => (Guid?)contact.CompanyId)
                .SingleOrDefaultAsync(cancellationToken);

            if (companyId is null)
            {
                return Result<bool>.Failure(CompaniesErrors.CompanyContactNotFound);
            }

            Domain.Company? company = await companies.GetByIdAsync(companyId.Value, cancellationToken);

            if (company is null)
            {
                return Result<bool>.Failure(CompaniesErrors.CompanyNotFound);
            }

            try
            {
                if (command.Approved)
                {
                    if (company.Visibility != CompanyVisibility.Shared)
                    {
                        return Result<bool>.Failure(CompaniesErrors.CompanyMustBeSharedBeforeContactApproval);
                    }

                    if (!company.IsSharedActiveLocation(command.CompanyLocationId))
                    {
                        return Result<bool>.Failure(CompaniesErrors.SharedContactRequiresSharedActiveLocation);
                    }

                    company.ApproveContact(
                        command.Id,
                        userContext.UserId,
                        DateTime.UtcNow,
                        command.Reason,
                        command.CompanyLocationId,
                        command.Name!,
                        command.RoleTitle,
                        command.Email,
                        command.PhoneNumber);
                }
                else
                {
                    company.RejectContact(
                        command.Id,
                        userContext.UserId,
                        DateTime.UtcNow,
                        command.Reason!);
                }

                await companies.SaveAsync(company, cancellationToken);

                return Result<bool>.Success(true);
            }
            catch (ArgumentException)
            {
                return Result<bool>.Failure(CompaniesErrors.CompanyLocationNotFound);
            }
            catch (InvalidOperationException)
            {
                return Result<bool>.Failure(CompaniesErrors.CompanyContactNotFound);
            }
        }
    }
}
