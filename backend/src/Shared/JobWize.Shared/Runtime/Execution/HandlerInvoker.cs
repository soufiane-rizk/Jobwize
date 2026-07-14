using JobWize.Shared.Application.Events;
using JobWize.Shared.Contracts.Application.Dispatching;
using JobWize.Shared.Contracts.Application.Events;
using JobWize.Shared.Runtime.Handlers;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Shared.Runtime.Execution
{
    public interface IHandlerInvoker<TResponse>
    {
        Task<TResponse> InvokeAsync(
            object handler,
            object request,
            CancellationToken cancellationToken);
    }

    internal sealed class HandlerInvoker<THandler, TRequest, TResponse>
        : IHandlerInvoker<TResponse>
        where THandler : IRequestHandler<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        public async Task<TResponse> InvokeAsync(
            object handler,
            object request,
            CancellationToken cancellationToken)
        {
            TResponse response =
                await ((THandler)handler).HandleAsync(
                    (TRequest)request,
                    cancellationToken);

            return response;
        }
    }

    internal sealed class IntegrationEventHandlerInvoker<THandler, TRequest>
        : IHandlerInvoker<object?>
        where THandler : IIntegrationEventHandler<TRequest>
        where TRequest : IIntegrationEvent
    {
        public async Task<object?> InvokeAsync(
            object handler,
            object request,
            CancellationToken cancellationToken)
        {
            await ((THandler)handler).HandleAsync(
                (TRequest)request,
                cancellationToken);

            return null;
        }
    }
}
