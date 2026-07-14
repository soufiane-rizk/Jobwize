using JobWize.Shared.Runtime.Execution;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace JobWize.Shared.Runtime.Discovery
{
    public sealed class ModuleDescriptor
    {
        public ImmutableArray<Type> Requests { get; init; } = [];

        public ImmutableArray<HandlerDescriptor> Handlers { get; init; } = [];

        public ImmutableArray<HandlerDescriptor> NotificationHandlers { get; init; } = [];
    }
}
