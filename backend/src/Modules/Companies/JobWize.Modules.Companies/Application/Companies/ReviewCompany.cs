using FluentValidation;
using JobWize.Modules.Companies.Contracts.Events.Companies;
using JobWize.Modules.Companies.Persistence;
using JobWize.Runtime.Contracts.Dispatching;
using JobWize.Shared.Application.Results;
using JobWize.Shared.Application.Security;
using JobWize.Shared.Endpoints;
using JobWize.Shared.Runtime.Contracts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace JobWize.Modules.Companies.Application.Companies;

public static class ReviewCompany
{
    internal sealed record Command(
        Guid Id,
        bool Approved,
        string? Reason,
        string? Name,
        string? Website,
        string? Industry,
        string? Description) : ICommand<bool>;

    internal sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(command => command.Reason).NotEmpty().MaximumLength(4000).When(command => !command.Approved);
            RuleFor(command => command.Name).MaximumLength(200);
            RuleFor(command => command.Website)
                .MaximumLength(2048)
                .Must(url => string.IsNullOrWhiteSpace(url) || Uri.TryCreate(url, UriKind.Absolute, out _));
            RuleFor(command => command.Industry).MaximumLength(200);
            RuleFor(command => command.Description).MaximumLength(8000);
        }
    }

    internal sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost(
                    Contracts.Public.Companies.ReviewCompany.Route,
                    async (
                        Guid id,
                        Contracts.Public.Companies.ReviewCompany.Request request,
                        IDispatcher dispatcher,
                        CancellationToken cancellationToken) =>
                    {
                        Result<bool> result = await dispatcher.SendAsync(
                            new Command(
                                id,
                                request.Approved,
                                request.Reason,
                                request.Name,
                                request.Website,
                                request.Industry,
                                request.Description),
                            cancellationToken);

                        return result.ToApiResult();
                    })
            .RequireAuthorization(global::JobWize.Modules.Identity.Contracts.Public.Authentication.AuthenticationPolicies.UserManagement)
            .WithName("ReviewCompany")
            .WithTags("Companies");
        }
    }

    internal sealed class Handler(ICompanyRepository companies, IUserContext userContext, IDispatcher dispatcher) : ICommandHandler<Command, bool>
    {
        public async Task<Result<bool>> HandleAsync(Command command, CancellationToken cancellationToken)
        {
            Domain.Company? company = await companies.GetByIdAsync(command.Id, cancellationToken);
            if (company is null)
            {
                return Result<bool>.Failure(CompaniesErrors.CompanyNotFound);
            }

            if (command.Approved)
            {
                company.UpdateBasicInformation(command.Name, command.Website, command.Industry, command.Description);
                company.Approve(userContext.UserId, DateTime.UtcNow, command.Reason);
                await dispatcher.PublishAsync(new CompanyPromotedToShared(company.Id, userContext.UserId), cancellationToken);
            }
            else
            {
                company.Reject(userContext.UserId, DateTime.UtcNow, command.Reason!);
                await dispatcher.PublishAsync(new CompanyReviewRejected(company.Id, userContext.UserId, command.Reason!), cancellationToken);
            }

            await companies.SaveAsync(company, cancellationToken);
            return Result<bool>.Success(true);
        }
    }
}
