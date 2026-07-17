using JobWize.Runtime.Contracts.Requests;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Runtime.Contracts.Pipelines
{
    public interface IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        Task<TResponse> HandleAsync(ExecutionContext<TRequest, TResponse> context, RequestExecutionDelegate<TResponse> next);
    }
}
