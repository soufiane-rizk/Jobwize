namespace JobWize.Frontend.Shared.Results
{
    public class Result
    {
        public bool IsSuccess { get; }

        public bool IsFailure => !IsSuccess;

        public Error? Error { get; }

        protected Result(bool isSuccess, Error? error)
        {
            IsSuccess = isSuccess;
            Error = error;
        }

        public static Result Success()
        {
            return new(true, null);
        }

        public static Result Failure(Error error)
        {
            return new(false, error);
        }
    }
}
