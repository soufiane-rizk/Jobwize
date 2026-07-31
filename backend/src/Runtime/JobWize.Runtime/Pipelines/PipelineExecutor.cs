using JobWize.Runtime.Contracts.Pipelines;
using JobWize.Runtime.Contracts.Requests;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Runtime.Pipelines
{
    internal interface IPipelineExecutor
    {
        Task<TResponse> ExecuteAsync<TRequest, TResponse>(ExecutionContext<TRequest, TResponse> context, RequestExecutionDelegate<TResponse> handler)
            where TRequest : IRequest<TResponse>;
    }


    internal sealed class PipelineExecutor : IPipelineExecutor
    {
        private readonly IPipelineResolver _pipelineResolver;

        public PipelineExecutor(IPipelineResolver pipelineResolver)
        {
            _pipelineResolver = pipelineResolver;
        }

        public Task<TResponse> ExecuteAsync<TRequest, TResponse>(ExecutionContext<TRequest, TResponse> context, RequestExecutionDelegate<TResponse> handler)
            where TRequest : IRequest<TResponse>
        {
            IEnumerable<IPipelineBehavior<TRequest, TResponse>> behaviors = _pipelineResolver.Resolve<TRequest, TResponse>(context.ServiceProvider);

            int count = behaviors.Count();

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
