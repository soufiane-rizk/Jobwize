using JobWize.Runtime.Contracts.Pipelines;
using JobWize.Runtime.Contracts.Requests;
using JobWize.Runtime.Discovery;
using JobWize.Runtime.Pipeline;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Runtime.Execution
{
    public interface IPipelineInvoker<TResponse>
    {
        Task<TResponse> InvokeAsync(HandlerDescriptor descriptor, IServiceProvider serviceProvider, object request, CancellationToken cancellationToken);
    }

    internal sealed class PipelineInvoker<THandler, TRequest, TResponse> : IPipelineInvoker<TResponse>
        where THandler : IRequestHandler<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        public async Task<TResponse> InvokeAsync(HandlerDescriptor descriptor, IServiceProvider serviceProvider, object request, CancellationToken cancellationToken)
        {
            TRequest typedRequest = (TRequest)request;

            ExecutionContext<TRequest, TResponse> context = new(typedRequest, serviceProvider, cancellationToken);

            IPipelineExecutor pipelineExecutor = serviceProvider.GetRequiredService<IPipelineExecutor>();

            object handler = serviceProvider.GetRequiredService(descriptor.HandlerType);

            IHandlerInvoker<TResponse> handlerInvoker = (IHandlerInvoker<TResponse>)descriptor.HandlerInvoker;

            RequestExecutionDelegate<TResponse> next = () => handlerInvoker.InvokeAsync(handler, typedRequest, cancellationToken);

            return await pipelineExecutor.ExecuteAsync(context, next);
        }
    }
}
