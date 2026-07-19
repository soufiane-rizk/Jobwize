using JobWize.ModuleOne.Contracts;
using JobWize.Runtime.Contracts.Dispatching;
using JobWize.Shared.Application.Results;
using JobWize.Shared.Runtime.Contracts;
using System;
using System.Collections.Generic;
using System.Text;
using static JobWize.ModuleTwo.Contracts.GetExternalItemSummary;

namespace JobWize.ModuleTwo.Features
{
    public static class GetExternalItemSummary
    {
        public sealed record Query(Guid Id) : IQuery<Response>;

        internal sealed class Handler: IQueryHandler<Query, Response>
        {
            private readonly IDispatcher _dispatcher;

            public Handler(IDispatcher dispatcher)
            {
                _dispatcher = dispatcher;
            }

            public async Task<Result<Response>> HandleAsync(
                Query request,
                CancellationToken cancellationToken)
            {
                ModuleOne.Contracts.GetItemSummary.Response summary =
                    await _dispatcher.SendModuleQueryAsync(
                        new GetItemSummary.Query(request.Id),
                        cancellationToken);

                return Result<Response>.Success(
                    new Response(summary.Id, summary.Name));
            }
        }
    }
}
