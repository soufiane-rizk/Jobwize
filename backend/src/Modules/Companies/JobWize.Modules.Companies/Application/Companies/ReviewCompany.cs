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
        string? Description,
        IReadOnlyList<Contracts.Public.Companies.ReviewCompany.Location>? Locations,
        IReadOnlyList<Contracts.Public.Companies.ReviewCompany.Contact>? Contacts) : ICommand<bool>;

    internal sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(command => command.Reason).NotEmpty().MaximumLength(4000).When(command => !command.Approved);
            RuleFor(command => command.Name)
                .NotEmpty()
                .MaximumLength(200)
                .When(command => command.Approved);
            RuleFor(command => command.Website)
                .MaximumLength(2048)
                .Must(url => string.IsNullOrWhiteSpace(url) || Uri.TryCreate(url, UriKind.Absolute, out _));
            RuleFor(command => command.Industry).MaximumLength(200);
            RuleFor(command => command.Description).MaximumLength(8000);
            RuleForEach(command => command.Locations)
                .ChildRules(location =>
                {
                    location.RuleFor(item => item.City).NotEmpty().MaximumLength(200);
                    location.RuleFor(item => item.Country).NotEmpty().MaximumLength(200);
                    location.RuleFor(item => item.Label).MaximumLength(200);
                    location.RuleFor(item => item.Address).MaximumLength(500);
                    location.RuleFor(item => item.Reason)
                        .NotEmpty()
                        .MaximumLength(4000)
                        .When(item => item.Id is not null && !item.Approved);
                })
                .When(command => command.Approved && command.Locations is not null);
            RuleForEach(command => command.Contacts)
                .ChildRules(contact =>
                {
                    contact.RuleFor(item => item.Name).NotEmpty().MaximumLength(200);
                    contact.RuleFor(item => item.RoleTitle).MaximumLength(200);
                    contact.RuleFor(item => item.Email).MaximumLength(320).EmailAddress();
                    contact.RuleFor(item => item.PhoneNumber).MaximumLength(50);
                    contact.RuleFor(item => item.Reason)
                        .NotEmpty()
                        .MaximumLength(4000)
                        .When(item => item.Id is not null && !item.Approved);
                })
                .When(command => command.Approved && command.Contacts is not null);
            RuleForEach(command => command.Contacts)
                .Must((command, contact) =>
                    contact.LocationIndex is null ||
                    (command.Locations is not null &&
                     contact.LocationIndex >= 0 &&
                     contact.LocationIndex < command.Locations.Count))
                .WithMessage("A contact location must refer to a submitted location.")
                .When(command => command.Approved && command.Contacts is not null);
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
                                request.Description,
                                request.Locations,
                                request.Contacts),
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
                DateTime reviewedAt = DateTime.UtcNow;

                company.ReplaceBasicInformation(
                    command.Name!,
                    command.Website,
                    command.Industry,
                    command.Description);
                company.Approve(
                    userContext.UserId,
                    reviewedAt,
                    command.Reason,
                    command.Locations is null && command.Contacts is null);

                var resolvedLocations = new List<Domain.CompanyLocation>();

                foreach (Contracts.Public.Companies.ReviewCompany.Location location in command.Locations ?? [])
                {
                    if (location.Id is null)
                    {
                        resolvedLocations.Add(company.AddSharedLocation(
                            location.Label,
                            location.City,
                            location.Country,
                            location.Address));
                        continue;
                    }

                    company.UpdateLocation(location.Id.Value, location.Label, location.City, location.Country, location.Address);
                    resolvedLocations.Add(company.Locations.Single(item => item.Id == location.Id.Value));

                    if (location.Approved)
                    {
                        company.ApproveLocation(location.Id.Value, userContext.UserId, reviewedAt, location.Reason);
                    }
                    else
                    {
                        company.RejectLocation(location.Id.Value, userContext.UserId, reviewedAt, location.Reason!);
                    }
                }

                foreach (Contracts.Public.Companies.ReviewCompany.Contact contact in command.Contacts ?? [])
                {
                    Guid? locationId = contact.LocationIndex is null
                        ? null
                        : resolvedLocations[contact.LocationIndex.Value].Id;

                    if (contact.Id is null)
                    {
                        company.AddSharedContact(
                            locationId,
                            contact.Name,
                            contact.RoleTitle,
                            contact.Email,
                            contact.PhoneNumber);
                        continue;
                    }

                    if (contact.Approved)
                    {
                        company.ApproveContact(
                            contact.Id.Value,
                            userContext.UserId,
                            reviewedAt,
                            contact.Reason,
                            locationId,
                            contact.Name,
                            contact.RoleTitle,
                            contact.Email,
                            contact.PhoneNumber);
                    }
                    else
                    {
                        company.UpdateContact(
                            contact.Id.Value,
                            locationId,
                            contact.Name,
                            contact.RoleTitle,
                            contact.Email,
                            contact.PhoneNumber);
                        company.RejectContact(contact.Id.Value, userContext.UserId, reviewedAt, contact.Reason!);
                    }
                }

                await companies.SaveAsync(company, cancellationToken);
                await dispatcher.PublishAsync(
                    new CompanyPromotedToShared(company.Id, userContext.UserId),
                    cancellationToken);
            }
            else
            {
                company.Reject(userContext.UserId, DateTime.UtcNow, command.Reason!);
                await companies.SaveAsync(company, cancellationToken);
                await dispatcher.PublishAsync(
                    new CompanyReviewRejected(
                        company.Id,
                        userContext.UserId,
                        command.Reason!),
                    cancellationToken);
            }

            return Result<bool>.Success(true);
        }
    }
}
