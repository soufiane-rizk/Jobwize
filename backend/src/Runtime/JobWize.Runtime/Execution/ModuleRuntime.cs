
using JobWize.Runtime.Contracts.Notifications;
using JobWize.Runtime.Contracts.Requests;
using JobWize.Runtime.Discovery;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Runtime.Execution
{
    public enum ExecutionScope { Global, Module }
    public interface IModuleRuntime
    {
        string Name { get; }

        IEnumerable<Type> DispatchableTypes { get; }

        Task<TResponse> SendAsync<TResponse>(IServiceProvider serviceProvider, IRequest<TResponse> request, CancellationToken cancellationToken);

        Task<TResponse> SendAsync<TResponse>(IServiceProvider serviceProvider, IModuleQuery<TResponse> query, CancellationToken cancellationToken);

        Task PublishAsync(IServiceProvider serviceProvider, INotification notification, ExecutionScope scope, CancellationToken cancellationToken = default);
    }

    public sealed class ModuleRuntime : IModuleRuntime
    {
        public string Name { get; }

        private readonly HandlerCatalog _handlerCatalog;

        public ModuleDescriptor Descriptor { get; }

        public IEnumerable<Type> DispatchableTypes =>  Descriptor.Requests.Concat(Descriptor.ModuleQueryHandlers.Select(x => x.RequestType));

        public ModuleRuntime(string name, ModuleDescriptor descriptor)
        {
            Name = name;
            Descriptor = descriptor;

            _handlerCatalog = new HandlerCatalog(descriptor);
        }

        public Task<TResponse> SendAsync<TResponse>(IServiceProvider serviceProvider, IRequest<TResponse> request, CancellationToken cancellationToken)
        {
            return SendCoreAsync<TResponse>(serviceProvider, request, cancellationToken);
        }

        public Task<TResponse> SendAsync<TResponse>(IServiceProvider serviceProvider, IModuleQuery<TResponse> query, CancellationToken cancellationToken)
        {
            return SendCoreAsync<TResponse>(serviceProvider, query, cancellationToken);
        }

        private Task<TResponse> SendCoreAsync<TResponse>(IServiceProvider serviceProvider, object message, CancellationToken cancellationToken)
        {
            HandlerDescriptor descriptor = _handlerCatalog.GetRequestHandler(message.GetType());

            object handler = serviceProvider.GetRequiredService(descriptor.HandlerType);

            var invoker = (IHandlerInvoker<TResponse>)descriptor.Invoker;

            return invoker.InvokeAsync(handler, message, cancellationToken);
        }

        public async Task PublishAsync(IServiceProvider serviceProvider, INotification notification, ExecutionScope scope, CancellationToken cancellationToken = default)
        {
            IReadOnlyCollection<HandlerDescriptor> descriptors = _handlerCatalog.GetNotificationHandlers(notification.GetType());

            foreach (HandlerDescriptor descriptor in descriptors)
            {
                object handler = serviceProvider.GetRequiredService(descriptor.HandlerType);

                var invoker = (IHandlerInvoker<object?>)descriptor.Invoker;

                await invoker.InvokeAsync(handler, notification, cancellationToken);
            }
        }
    }
}
