using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Shared.Application.Results
{
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
}
