
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
            List<HandlerDescriptor> moduleQueryHandlers = [];
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

                if (ImplementsModuleQuery(type))
                {
                    requests.Add(type);
                }

                foreach (Type implementedInterface in type.GetInterfaces())
                {
                    if (!implementedInterface.IsGenericType)
                    {
                        continue;
                    }

                    Type genericDefinition = implementedInterface.GetGenericTypeDefinition();

                    if (genericDefinition == typeof(IRequestHandler<,>))
                    {
                        RegisterRequestHandler(type, implementedInterface, handlers);
                    }
                    else if (genericDefinition == typeof(IModuleQueryHandler<,>))
                    {
                        RegisterModuleQueryHandler(type, implementedInterface, moduleQueryHandlers);
                    }
                    else if (genericDefinition == typeof(INotificationHandler<>))
                    {
                        RegisterNotificationHandler(type, implementedInterface, notificationHandlers);
                    }
                }
            }

            return new ModuleDescriptor
            {
                Requests = requests
                    .Distinct()
                    .OrderBy(x => x.FullName)
                    .ToImmutableArray(),

                Handlers = handlers
                    .OrderBy(x => x.RequestType.FullName)
                    .ToImmutableArray(),

                ModuleQueryHandlers = moduleQueryHandlers
                    .OrderBy(x => x.RequestType.FullName)
                    .ToImmutableArray(),

                NotificationHandlers = notificationHandlers
                    .OrderBy(x => x.RequestType.FullName)
                    .ToImmutableArray(),
            };
        }

        private static bool ImplementsModuleQuery(Type type)
        {
            return type.GetInterfaces()
                .Any(i =>
                    i.IsGenericType &&
                    i.GetGenericTypeDefinition() == typeof(IModuleQuery<>));
        }

        private static void RegisterRequestHandler(Type handlerType, Type implementedInterface, List<HandlerDescriptor> handlers)
        {
            Type[] genericArguments = implementedInterface.GetGenericArguments();

            Type requestType = genericArguments[0];
            Type responseType = genericArguments[1];

            Type invokerType =
                typeof(HandlerInvoker<,,>).MakeGenericType(
                    handlerType,
                    requestType,
                    responseType);

            object invoker = Activator.CreateInstance(invokerType)!;

            handlers.Add(
                new HandlerDescriptor(
                    requestType,
                    handlerType,
                    invoker));
        }

        private static void RegisterModuleQueryHandler(Type handlerType, Type implementedInterface, List<HandlerDescriptor> handlers)
        {
            Type[] genericArguments = implementedInterface.GetGenericArguments();

            Type requestType = genericArguments[0];
            Type responseType = genericArguments[1];

            Type invokerType =
                typeof(ModuleQueryHandlerInvoker<,,>).MakeGenericType(
                    handlerType,
                    requestType,
                    responseType);

            object invoker = Activator.CreateInstance(invokerType)!;

            handlers.Add(
                new HandlerDescriptor(
                    requestType,
                    handlerType,
                    invoker));
        }

        private static void RegisterNotificationHandler(Type handlerType, Type implementedInterface, List<HandlerDescriptor> notificationHandlers)
        {
            Type requestType = implementedInterface.GetGenericArguments()[0];

            Type invokerType =
                typeof(IntegrationEventHandlerInvoker<,>).MakeGenericType(
                    handlerType,
                    requestType);

            object invoker = Activator.CreateInstance(invokerType)!;

            notificationHandlers.Add(
                new HandlerDescriptor(
                    requestType,
                    handlerType,
                    invoker));
        }
    }
}