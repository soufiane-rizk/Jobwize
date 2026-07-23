using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Shared.Application.Results
{
    public enum ErrorType
    {
        Validation,
        Conflict,
        NotFound,
        Unauthorized,
        Forbidden,
        Failure
    }
}
