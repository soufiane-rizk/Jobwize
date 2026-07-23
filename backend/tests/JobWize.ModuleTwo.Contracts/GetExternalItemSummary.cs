using JobWize.Shared.Runtime.Contracts;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

[assembly: InternalsVisibleTo("JobWize.Runtime.UnitTests")]

namespace JobWize.ModuleTwo.Contracts
{
    public static class GetExternalItemSummary
    {
        public sealed record Response(Guid Id, string Name);
    }
}
