using JobWize.Frontend.Shared.Results;

namespace JobWize.Frontend.Shared.Results
{
    public sealed class Result<T> : Result
    {
        public T? Value { get; }

        private Result(T value)
            : base(true, null)
        {
            Value = value;
        }

        private Result(Error error)
            : base(false, error)
        {
        }

        public static Result<T> Success(T value)
        {
            return new(value);
        }

        public static new Result<T> Failure(Error error)
        {
            return new(error);
        }
    }
}
