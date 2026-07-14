using JobWize.Shared.Contracts.Application.Dispatching;
using JobWize.Shared.Contracts.Application.Events;
using JobWize.Shared.Runtime.Discovery;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Shared.Runtime.Execution
{
    public sealed class ModuleRuntime
    {
        public string Name { get; init; }
        private readonly HandlerCatalog _handlerCatalog;
        public ModuleDescriptor Descriptor { get; }

        public IReadOnlyCollection<Type> RequestTypes => Descriptor.Requests;

        public ModuleRuntime(string name, ModuleDescriptor descriptor)
        {
            Name = name;    
            Descriptor = descriptor;
            _handlerCatalog = new HandlerCatalog(descriptor);
        }

        public Task<TResponse> SendAsync<TResponse>(IServiceProvider serviceProvider, IRequest<TResponse> request, CancellationToken cancellationToken)
        {
            HandlerDescriptor descriptor = _handlerCatalog.GetRequestHandler(request.GetType());

            object handler = serviceProvider.GetRequiredService(descriptor.HandlerType);

            var invoker = (IHandlerInvoker<TResponse>)descriptor.Invoker;

            return invoker.InvokeAsync(
                handler,
                request,
                cancellationToken);
        }

        public async Task PublishAsync(IServiceProvider serviceProvider, IIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
        {
            var descriptors = _handlerCatalog.GetNotificationHandlers(integrationEvent.GetType());

            foreach (var descriptor in descriptors)
            {
                object handler = serviceProvider.GetRequiredService(descriptor.HandlerType);
                var invoker = (IHandlerInvoker<object?>)descriptor.Invoker;
                await invoker.InvokeAsync(handler, integrationEvent, cancellationToken);
            }
        }
    }
}
