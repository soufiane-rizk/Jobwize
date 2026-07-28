using JobWize.Runtime.Contracts.Pipelines;
using JobWize.Runtime.Contracts.Requests;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Runtime.Execution
{
    internal interface IPipelineExecutor
    {
        Task<TResponse> ExecuteAsync<TRequest, TResponse>(ExecutionContext<TRequest, TResponse> context, RequestExecutionDelegate<TResponse> handler)
            where TRequest : IRequest<TResponse>;
    }


    internal sealed class PipelineExecutor : IPipelineExecutor
    {
        public Task<TResponse> ExecuteAsync<TRequest, TResponse>(ExecutionContext<TRequest, TResponse> context, RequestExecutionDelegate<TResponse> handler)
            where TRequest : IRequest<TResponse>
        {
            IEnumerable<IPipelineBehavior<TRequest, TResponse>> behaviors = context.ServiceProvider.GetServices<IPipelineBehavior<TRequest, TResponse>>();

            RequestExecutionDelegate<TResponse> next = handler;

            foreach (IPipelineBehavior<TRequest, TResponse> behavior in behaviors.Reverse())
            {
                RequestExecutionDelegate<TResponse> current = next;

                next = () => behavior.HandleAsync(context, current);
            }

            return next();
        }
    }
}
