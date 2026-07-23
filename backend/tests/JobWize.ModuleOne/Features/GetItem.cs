using JobWize.Shared.Application.Results;
using JobWize.Shared.Runtime.Contracts;
using static JobWize.ModuleOne.Contracts.GetItem;

namespace JobWize.ModuleOne.Features;

public static class GetItem
{
    public sealed record Query(Guid Id) : IQuery<Response>;

    internal sealed class Handler : IQueryHandler<Query, Response>
    {
        public Task<Result<Response>> HandleAsync(Query request, CancellationToken cancellationToken)
        {
            Response item = new(request.Id, "Test Item");

            return Task.FromResult(Result<Response>.Success(item));
        }
    }
}