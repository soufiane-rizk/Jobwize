using JobWize.Runtime.Discovery;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Runtime.Registration
{
    public static class ModuleValidator
    {
        public static void Validate(ModuleDescriptor descriptor)
        {
            ValidateRequestsHaveExactlyOneHandler(descriptor);

            ValidateDispatchableTypesHaveExactlyOneHandler(descriptor);
        }

        private static void ValidateRequestsHaveExactlyOneHandler(ModuleDescriptor descriptor)
        {
            foreach (Type requestType in descriptor.Requests)
            {
                int count = descriptor.Handlers.Count(
                    x => x.RequestType == requestType);

                if (count == 0)
                {
                    throw new InvalidOperationException(
                        $"Request '{requestType.FullName}' has no handler.");
                }

                if (count > 1)
                {
                    throw new InvalidOperationException(
                        $"Request '{requestType.FullName}' has multiple handlers.");
                }
            }
        }

        private static void ValidateDispatchableTypesHaveExactlyOneHandler(ModuleDescriptor descriptor)
        {
            HashSet<Type> requests = descriptor.Requests.ToHashSet();

            foreach (HandlerDescriptor handler in descriptor.Handlers)
            {
                if (!requests.Contains(handler.RequestType))
                {
                    throw new InvalidOperationException(
                        $"Handler '{handler.HandlerType.FullName}' targets '{handler.RequestType.FullName}', which is not a valid IRequest.");
                }
            }
        }
    }
}
