using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace JobWize.Shared.Runtime.Execution
{
    public sealed record HandlerDescriptor(
        Type RequestType,
        Type HandlerType,
        object Invoker);
}
