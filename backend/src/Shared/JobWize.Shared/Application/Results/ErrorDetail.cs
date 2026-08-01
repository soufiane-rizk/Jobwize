using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Shared.Application.Results
{
    public sealed record ErrorDetail(string Field, string Message);
}
