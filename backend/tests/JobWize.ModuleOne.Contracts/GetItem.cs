using JobWize.Shared.Runtime.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.ModuleOne.Contracts
{
    public static class GetItem
    {
        public sealed record Response(Guid Id, string Name);
    }
}
