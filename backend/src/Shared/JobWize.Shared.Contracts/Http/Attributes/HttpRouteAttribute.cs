using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Shared.Contracts.Http.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]

    public sealed class HttpRouteAttribute : Attribute
    {
    }
}
