using JobWize.Shared.Errors;

namespace JobWize.Modules.Identity.Domain;

public static class DomainErrors
{
    public static readonly Error RequiredValue = new(
        "Identity.Domain.RequiredValue",
        "The required value was not provided.",
        ErrorType.Validation);
}
