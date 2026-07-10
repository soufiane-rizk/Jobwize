using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Shared.Application.Results
{
    public sealed record Error(string Code, string Message, ErrorType Type);
}
