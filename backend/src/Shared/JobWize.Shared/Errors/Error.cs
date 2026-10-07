namespace JobWize.Shared.Errors;

public sealed record Error(
    string Code,
    string Message,
    ErrorType Type,
    IReadOnlyCollection<ErrorDetail>? Details = null);
