using JobWize.Runtime.Contracts.Requests;
using JobWize.Shared.Application.Results;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.ModuleOne.Contracts
{
    public static class GetItemSummary
    {
        public sealed record Query(Guid Id) : IModuleQuery<Response>;

        public sealed record Response(Guid Id, string Name);
    }
}
