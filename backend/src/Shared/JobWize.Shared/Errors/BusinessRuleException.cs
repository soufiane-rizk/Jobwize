namespace JobWize.Shared.Errors;

public sealed class BusinessRuleException : Exception
{
    public BusinessRuleException(Error error)
        : base(GetMessage(error))
    {
        Error = error;
    }

    public Error Error { get; }

    private static string GetMessage(Error? error)
    {
        ArgumentNullException.ThrowIfNull(error);
        return error.Message;
    }
}
