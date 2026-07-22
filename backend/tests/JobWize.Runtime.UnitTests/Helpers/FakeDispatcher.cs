using JobWize.Runtime.Contracts.Dispatching;
using JobWize.Runtime.Contracts.Notifications;
using JobWize.Runtime.Contracts.Requests;
using JobWize.Shared.Application.Results;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Runtime.UnitTests.Helpers
{
    internal sealed class FakeDispatcher : IDispatcher
    {
        public object? PublishedNotification { get; private set; }

        public Task PublishAsync(INotification notification, CancellationToken cancellationToken = default)
        {
            PublishedNotification = notification;

            return Task.CompletedTask;
        }

        public Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken)
             => throw new NotSupportedException();

        public Task<TResponse> SendModuleQueryAsync<TResponse>(IModuleQuery<TResponse> query, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();  
    }
}
