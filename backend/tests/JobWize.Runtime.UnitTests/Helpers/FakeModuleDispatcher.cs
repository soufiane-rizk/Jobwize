using JobWize.Runtime.Contracts.Requests;
using JobWize.Runtime.Dispatching;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Runtime.UnitTests.Helpers
{
    internal sealed class FakeModuleDispatcher : IModuleDispatcher
    {
        public object? Response { get; set; }

        public object? Query { get; private set; }
        public Type? QueryType { get; private set; }

        public Task<TResponse> SendAsync<TResponse>(IModuleQuery<TResponse> query, CancellationToken cancellationToken = default)
        {
            Query = query;
            QueryType = query.GetType();

            return Task.FromResult((TResponse)Response!);
        }
    }
}
