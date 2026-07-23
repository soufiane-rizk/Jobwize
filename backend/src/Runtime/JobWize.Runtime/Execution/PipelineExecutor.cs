using JobWize.Runtime.Contracts.Pipelines;
using JobWize.Runtime.Contracts.Requests;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Runtime.Pipeline
{
    public interface IPipelineExecutor
    {
        public interface IPipelineExecutor
        {
            Task<TResponse> ExecuteAsync<TRequest, TResponse>(
                ExecutionContext<TRequest, TResponse> context,
                IEnumerable<IPipelineBehavior<TRequest, TResponse>> behaviors,
                RequestExecutionDelegate<TResponse> handlerDelegate)
                where TRequest : IRequest<TResponse>;
        }
    }

    public sealed class PipelineExecutor : IPipelineExecutor
    {
        public Task<TResponse> ExecuteAsync<TRequest, TResponse>(
            ExecutionContext<TRequest, TResponse> context,
            IEnumerable<IPipelineBehavior<TRequest, TResponse>> behaviors,
            RequestExecutionDelegate<TResponse> handlerDelegate)
            where TRequest : IRequest<TResponse>
        {
            RequestExecutionDelegate<TResponse> pipeline = handlerDelegate;

            foreach (var behavior in behaviors.Reverse())
            {
                var next = pipeline;

                pipeline = () =>
                    behavior.HandleAsync(context, next);
            }

            return pipeline();
        }
    }
}
