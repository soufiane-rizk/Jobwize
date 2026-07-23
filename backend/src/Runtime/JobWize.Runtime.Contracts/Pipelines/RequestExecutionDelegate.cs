using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Runtime.Contracts.Pipelines
{
    public delegate Task<TResponse> RequestExecutionDelegate<TResponse>();
}
