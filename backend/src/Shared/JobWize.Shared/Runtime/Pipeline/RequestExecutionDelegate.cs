using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Shared.Runtime.Pipeline
{
    public delegate Task<TResponse> RequestExecutionDelegate<TResponse>();
}
