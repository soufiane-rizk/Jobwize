
using JobWize.Runtime.Contracts.Notifications;
using JobWize.Runtime.Contracts.Pipelines;
using JobWize.Runtime.Contracts.Requests;
using JobWize.Runtime.Discovery;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Runtime.Execution
{
    public interface IModuleRuntime
    {
        string Name { get; }

        IEnumerable<Type> DispatchableTypes { get; }
        IEnumerable<Type> NotificationTypes { get; }

        Task<TResponse> SendAsync<TResponse>(IServiceProvider serviceProvider, IRequest<TResponse> request, CancellationToken cancellationToken);

        Task<TResponse> SendAsync<TResponse>(IServiceProvider serviceProvider, IModuleQuery<TResponse> query, CancellationToken cancellationToken);

        Task PublishAsync(IServiceProvider serviceProvider, INotification notification, CancellationToken cancellationToken = default);
    }

    public sealed class ModuleRuntime : IModuleRuntime
    {
        public string Name { get; }

        private readonly HandlerCatalog _handlerCatalog;

        public ModuleDescriptor Descriptor { get; }

        public IEnumerable<Type> DispatchableTypes =>  Descriptor.Requests.Concat(Descriptor.ModuleQueryHandlers.Select(x => x.RequestType));

        public IEnumerable<Type> NotificationTypes => Descriptor.NotificationHandlers.Select(x => x.RequestType).Distinct();

        public ModuleRuntime(string name, ModuleDescriptor descriptor)
        {
            Name = name;
            Descriptor = descriptor;

            _handlerCatalog = new HandlerCatalog(descriptor);
        }

        public Task<TResponse> SendAsync<TResponse>(IServiceProvider serviceProvider, IRequest<TResponse> request, CancellationToken cancellationToken)
        {
            HandlerDescriptor descriptor = _handlerCatalog.GetRequestHandler(request.GetType());

            var pipelineInvoker = (IPipelineInvoker<TResponse>)descriptor.PipelineInvoker!;

            return pipelineInvoker.InvokeAsync(descriptor, serviceProvider, request, cancellationToken);

        }

        public Task<TResponse> SendAsync<TResponse>(IServiceProvider serviceProvider, IModuleQuery<TResponse> query, CancellationToken cancellationToken)
        {
            HandlerDescriptor descriptor = _handlerCatalog.GetRequestHandler(query.GetType());

            var handler = serviceProvider.GetRequiredService(descriptor.HandlerType);

            var invoker = (IHandlerInvoker<TResponse>)descriptor.HandlerInvoker;

            return invoker.InvokeAsync(handler, query, cancellationToken);
        }

        public async Task PublishAsync(IServiceProvider serviceProvider, INotification notification, CancellationToken cancellationToken = default)
        {
            IReadOnlyCollection<HandlerDescriptor> descriptors = _handlerCatalog.GetNotificationHandlers(notification.GetType());

            foreach (HandlerDescriptor descriptor in descriptors)
            {
                object handler = serviceProvider.GetRequiredService(descriptor.HandlerType);

                var invoker = (IHandlerInvoker<object?>)descriptor.HandlerInvoker;

                await invoker.InvokeAsync(handler, notification, cancellationToken);
            }
        }
    }
}
