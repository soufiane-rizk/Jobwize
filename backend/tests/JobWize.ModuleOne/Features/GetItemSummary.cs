using JobWize.ModuleOne.Contracts;
using JobWize.Runtime.Contracts.Requests;
using JobWize.Shared.Application.Results;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.ModuleOne.Features
{
    internal sealed class Handler : IModuleQueryHandler<GetItemSummary.Query, GetItemSummary.Response>
    {
        private readonly IItemRepository _repository;

        public Handler(IItemRepository repository)
        {
            _repository = repository;
        }

        public Task<GetItemSummary.Response> HandleAsync(GetItemSummary.Query request, CancellationToken cancellationToken)
        {
            GetItemSummary.Response response = new(request.Id, "Test Item");

            return Task.FromResult(response);
        }
    }
}
