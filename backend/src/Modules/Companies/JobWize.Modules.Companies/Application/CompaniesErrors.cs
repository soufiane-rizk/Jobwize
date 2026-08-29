using JobWize.Shared.Application.Results;

namespace JobWize.Modules.Companies.Application;

public static class CompaniesErrors
{
    public static readonly Error CompanyNotFound = new("Companies.CompanyNotFound", "The requested company was not found.", ErrorType.NotFound);

    public static readonly Error CompanyLocationNotFound = new(
        "Companies.CompanyLocationNotFound",
        "The selected company location was not found.",
        ErrorType.Validation);

    public static readonly Error CompanyContactNotFound = new(
        "Companies.CompanyContactNotFound",
        "The requested company contact was not found.",
        ErrorType.NotFound);

    public static readonly Error CompanyMustBeSharedBeforeContactApproval = new(
        "Companies.CompanyMustBeSharedBeforeContactApproval",
        "Approve the company before sharing one of its contacts.",
        ErrorType.Validation);

    public static readonly Error SharedContactRequiresSharedActiveLocation = new(
        "Companies.SharedContactRequiresSharedActiveLocation",
        "A shared contact can only use an active shared company location.",
        ErrorType.Validation);
}
