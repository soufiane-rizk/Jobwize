using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Shared.Application.Results
{
    public class Result
    {
        protected Result(bool isSuccess, Error error)
        {
            if (isSuccess && error != SharedErrors.None)
                throw new ArgumentException("Successful results cannot contain an error.");

            if (!isSuccess && error == SharedErrors.None)
                throw new ArgumentException("Failed results must contain an error.");

            IsSuccess = isSuccess;
            Error = error;
        }

        public bool IsSuccess { get; }

        public bool IsFailure => !IsSuccess;

        public Error Error { get; }

        public static Result Success()
            => new(true, SharedErrors.None);

        public static Result Failure(Error error)
            => new(false, error);
    }
}
