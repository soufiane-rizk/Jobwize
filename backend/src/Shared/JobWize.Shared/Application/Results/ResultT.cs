using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Shared.Application.Results
{
    public sealed class Result<T> : Result
    {
        private readonly T? _value;

        private Result(T value)
            : base(true, SharedErrors.None)
        {
            _value = value;
        }

        private Result(Error error)
            : base(false, error)
        {
            _value = default;
        }

        public T Value =>
            IsSuccess
                ? _value!
                : throw new InvalidOperationException("A failed result has no value.");

        public static Result<T> Success(T value)
            => new(value);

        public static new Result<T> Failure(Error error)
            => new(error);
    }
}
