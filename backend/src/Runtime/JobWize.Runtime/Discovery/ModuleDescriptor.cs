using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace JobWize.Runtime.Discovery
{
    public sealed class ModuleDescriptor
    {
        public ImmutableArray<Type> Requests { get; init; } = [];

        public ImmutableArray<HandlerDescriptor> Handlers { get; init; } = [];

        public ImmutableArray<HandlerDescriptor> NotificationHandlers { get; init; } = [];
    }
}
