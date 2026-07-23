
using JobWize.Runtime.Contracts.Notifications;
using JobWize.Runtime.Contracts.Requests;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Runtime.Execution
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
        where THandler : INotificationHandler<TRequest>
        where TRequest : INotification
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
    
    internal sealed class ModuleQueryHandlerInvoker<THandler, TQuery, TResponse>
        : IHandlerInvoker<TResponse>
        where THandler : IModuleQueryHandler<TQuery, TResponse>
        where TQuery : IModuleQuery<TResponse>
    {
        public async Task<TResponse> InvokeAsync(
            object handler,
            object request,
            CancellationToken cancellationToken)
        {
            return await ((THandler)handler).HandleAsync(
                (TQuery)request,
                cancellationToken);
        }
    }
}
