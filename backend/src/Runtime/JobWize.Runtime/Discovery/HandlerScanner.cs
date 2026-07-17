
using JobWize.Runtime.Contracts.Notifications;
using JobWize.Runtime.Contracts.Requests;
using JobWize.Runtime.Execution;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace JobWize.Runtime.Discovery
{
    internal interface IHandlerScanner
    {
        ModuleDescriptor Scan(Assembly assembly);
    }

    public sealed class HandlerScanner : IHandlerScanner
    {
        public ModuleDescriptor Scan(Assembly assembly)
        {
            List<Type> requests = [];
            List<HandlerDescriptor> handlers = [];
            List<HandlerDescriptor> notificationHandlers = [];

            foreach (Type type in assembly.GetTypes())
            {
                if (!type.IsClass ||
                    type.IsAbstract ||
                    type.IsGenericTypeDefinition)
                {
                    continue;
                }

                if (typeof(IRequest).IsAssignableFrom(type))
                {
                    requests.Add(type);
                }

                foreach (Type implementedInterface in type.GetInterfaces())
                {
                    if (!implementedInterface.IsGenericType)
                    {
                        continue;
                    }

                    if (implementedInterface.GetGenericTypeDefinition() == typeof(IRequestHandler<,>))
                    {
                        Type[] genericArguments = implementedInterface.GetGenericArguments();

                        Type requestType = genericArguments[0];
                        Type responseType = genericArguments[1];

                        Type invokerType =
                            typeof(HandlerInvoker<,,>).MakeGenericType(
                                type,
                                requestType,
                                responseType);

                        object invoker = Activator.CreateInstance(invokerType)!;

                        handlers.Add(
                            new HandlerDescriptor(
                                requestType,
                                type,
                                invoker));
                    }

                    else if (implementedInterface.GetGenericTypeDefinition() == typeof(INotificationHandler<>))
                    {
                        Type[] genericArguments = implementedInterface.GetGenericArguments();

                        Type requestType = genericArguments[0];

                        Type invokerType =
                            typeof(IntegrationEventHandlerInvoker<,>).MakeGenericType(
                                type,
                                requestType);   

                        object invoker = Activator.CreateInstance(invokerType)!;

                        notificationHandlers.Add(
                            new HandlerDescriptor(
                                requestType,
                                type,
                                invoker));
                    }
                }
            }

            return new ModuleDescriptor
            {
                Requests = requests
                    .OrderBy(x => x.FullName)
                    .ToImmutableArray(),

                Handlers = handlers
                    .OrderBy(x => x.RequestType.FullName)
                    .ToImmutableArray(),

                NotificationHandlers = notificationHandlers
                    .OrderBy(x => x.RequestType.FullName)
                    .ToImmutableArray(),
            };
        }
    }
}
