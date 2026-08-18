namespace JobWize.Frontend.Shared.Results
{
    public sealed record Error(string Code, string Message, ErrorType Type, Dictionary<string, string[]>? ValidationErrors = null)
    {
        public bool IsValidation => Type == ErrorType.Validation;

        public bool IsConflict => Type == ErrorType.Conflict;

        public bool IsNotFound => Type == ErrorType.NotFound;

        public bool IsUnauthorized => Type == ErrorType.Unauthorized;

        public bool IsForbidden => Type == ErrorType.Forbidden;

        public bool IsUnexpected => Type == ErrorType.Unexpected;
    }
}
