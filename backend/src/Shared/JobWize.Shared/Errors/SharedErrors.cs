namespace JobWize.Shared.Errors;

public static class SharedErrors
{
    public static readonly Error None = new(
        string.Empty,
        string.Empty,
        ErrorType.Failure);

    public static readonly Error Unexpected = new(
        "Shared.UnexpectedError",
        "An unexpected error occurred while processing the request.",
        ErrorType.Failure);
}
