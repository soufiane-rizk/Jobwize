using JobWize.Shared.Contracts.Application.Dispatching;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Shared.Runtime.Pipeline
{
    public interface IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        Task<TResponse> HandleAsync(ExecutionContext<TRequest, TResponse> context, RequestExecutionDelegate<TResponse> next);
    }
}
