using JobWize.Shared.Application.Events;
using JobWize.Shared.Application.Results;
using JobWize.Shared.Contracts.Application.Dispatching;
using JobWize.Shared.Contracts.Application.Events;
using JobWize.Shared.Runtime.Execution;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace JobWize.Shared.Runtime.Discovery
{
    public interface IHandlerCatalog
    {
        HandlerDescriptor GetRequestHandler(Type requestType);
        IReadOnlyCollection<HandlerDescriptor> GetNotificationHandlers(Type notificationType);
    }

    public sealed class HandlerCatalog : IHandlerCatalog
    {
        private readonly ImmutableDictionary<Type, HandlerDescriptor> _requestHandlers;

        private readonly ImmutableDictionary<Type, IReadOnlyCollection<HandlerDescriptor>> _notificationHandlers;

        public HandlerCatalog(ModuleDescriptor moduleDescriptor)
        {
            var requestHandlers = moduleDescriptor.Handlers.ToDictionary(x => x.RequestType);
            var notificationHandlers = moduleDescriptor.NotificationHandlers
                .GroupBy(x => x.RequestType)
                .ToDictionary(x => x.Key, x => (IReadOnlyCollection<HandlerDescriptor>)x.ToList());

            _requestHandlers = requestHandlers.ToImmutableDictionary();
            _notificationHandlers = notificationHandlers.ToImmutableDictionary();
        }

        public HandlerDescriptor GetRequestHandler(Type requestType)
        {
            if (_requestHandlers.TryGetValue(requestType, out HandlerDescriptor? descriptor))
            {
                return descriptor;
            }

            throw new InvalidOperationException(
                $"No handler registered for request '{requestType.FullName}'.");
        }

        public IReadOnlyCollection<HandlerDescriptor> GetNotificationHandlers(Type notificationType)
        {
            if (_notificationHandlers.TryGetValue(notificationType, out IReadOnlyCollection<HandlerDescriptor>? descriptors))
            {
                return descriptors;
            }

            return Array.Empty<HandlerDescriptor>();
        }
    }

}