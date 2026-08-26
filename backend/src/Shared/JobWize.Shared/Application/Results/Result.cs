using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Shared.Application.Results
{
    public class Result
    {
        protected Result(bool isSuccess, Error error, IReadOnlyList<Confirmation>? confirmations = null)
        {
            if (isSuccess && error != SharedErrors.None)
                throw new ArgumentException("Successful results cannot contain an error.");

            if (!isSuccess && error == SharedErrors.None)
                throw new ArgumentException("Failed results must contain an error.");

            IsSuccess = isSuccess;
            Error = error;
            Confirmations = confirmations ?? [];
        }

        public bool IsSuccess { get; }

        public bool IsFailure => !IsSuccess;

        public Error Error { get; }
        public IReadOnlyList<Confirmation> Confirmations { get; }
        public bool NeedsConfirmation => Confirmations.Count > 0;

        public static Result Success()
            => new(true, SharedErrors.None);

        public static Result Failure(Error error)
            => new(false, error);

        public static Result ConfirmationRequired(Confirmation confirmation)
            => new(false, new Error("Confirmation.Required", confirmation.Message, ErrorType.ConfirmationRequired), [confirmation]);
    }
}
