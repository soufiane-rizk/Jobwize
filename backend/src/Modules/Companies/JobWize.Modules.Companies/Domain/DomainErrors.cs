using JobWize.Shared.Errors;

namespace JobWize.Modules.Companies.Domain;

public static class DomainErrors
{
    public static readonly Error CompanyNameRequired = new(
        "Companies.Domain.CompanyNameRequired",
        "A company name is required.",
        ErrorType.Validation);

    public static readonly Error CompanyContactNameRequired = new(
        "Companies.Domain.CompanyContactNameRequired",
        "A company contact name is required.",
        ErrorType.Validation);

    public static readonly Error ReviewReasonRequired = new(
        "Companies.Domain.ReviewReasonRequired",
        "A review reason is required.",
        ErrorType.Validation);

    public static readonly Error CompanyCannotBeReviewedAgain = new(
        "Companies.Domain.CompanyCannotBeReviewedAgain",
        "A shared company cannot be reviewed again.",
        ErrorType.Validation);

    public static readonly Error CompanyContactCannotBeReviewedAgain = new(
        "Companies.Domain.CompanyContactCannotBeReviewedAgain",
        "A shared company contact cannot be reviewed again.",
        ErrorType.Validation);

    public static readonly Error CompanyMustBeSharedBeforeContactApproval = new(
        "Companies.Domain.CompanyMustBeSharedBeforeContactApproval",
        "A company must be shared before one of its contacts can be approved.",
        ErrorType.Validation);

    public static readonly Error LocationNotInCompany = new(
        "Companies.Domain.LocationNotInCompany",
        "The selected location does not belong to this company.",
        ErrorType.Validation);

    public static readonly Error LocationNotSelectable = new(
        "Companies.Domain.LocationNotSelectable",
        "The selected location is not selectable for this candidate.",
        ErrorType.Validation);

    public static readonly Error LocationCityRequired = new(
        "Companies.Domain.LocationCityRequired",
        "A location city is required.",
        ErrorType.Validation);

    public static readonly Error LocationCountryRequired = new(
        "Companies.Domain.LocationCountryRequired",
        "A location country is required.",
        ErrorType.Validation);

    public static readonly Error SharedContactRequiresActiveSharedLocation = new(
        "Companies.Domain.SharedContactRequiresActiveSharedLocation",
        "A shared contact requires an active shared location.",
        ErrorType.Validation);

    public static readonly Error CompanyContactNotInCompany = new(
        "Companies.Domain.CompanyContactNotInCompany",
        "The selected company contact does not belong to this company.",
        ErrorType.NotFound);

    public static readonly Error CompanyLocationNotInCompany = new(
        "Companies.Domain.CompanyLocationNotInCompany",
        "The selected company location does not belong to this company.",
        ErrorType.NotFound);
}
