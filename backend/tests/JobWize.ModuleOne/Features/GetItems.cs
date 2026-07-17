using JobWize.Shared.Application.Results;
using JobWize.Shared.Contracts.Application.Dispatching;
using JobWize.Shared.Runtime.Handlers;

namespace JobWize.ModuleOne.Features;

public static class GetItem
{
    public sealed record Query(Guid Id) : IQuery<ItemDto>;

    public sealed record ItemDto(Guid Id, string Name);

    internal sealed class Handler : IQueryHandler<Query, ItemDto>
    {
        public Task<Result<ItemDto>> HandleAsync(Query request, CancellationToken cancellationToken)
        {
            ItemDto item = new(request.Id, "Test Item");

            return Task.FromResult(Result<ItemDto>.Success(item));
        }
    }
}