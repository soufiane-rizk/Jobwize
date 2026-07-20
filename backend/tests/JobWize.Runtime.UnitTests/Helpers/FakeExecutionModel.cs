using JobWize.Runtime.Contracts.Notifications;
using JobWize.Runtime.Contracts.Requests;
using JobWize.Runtime.Execution;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Runtime.UnitTests.Helpers
{
    internal sealed class FakeExecutionModel : IExecutionModel
    {
        public IRequest? Request { get; private set; }

        public object? RequestWithResponse { get; private set; }

        public object? ModuleQuery { get; private set; }

        public INotification? Notification { get; private set; }

        public object? Response { get; set; }

        public Task SendAsync(IRequest request, CancellationToken cancellationToken = default)
        {
            Request = request;

            return Task.CompletedTask;
        }

        public Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            RequestWithResponse = request;

            return Task.FromResult((TResponse)Response!);
        }

        public Task<TResponse> SendAsync<TResponse>(IModuleQuery<TResponse> query, CancellationToken cancellationToken = default)
        {
            ModuleQuery = query;

            return Task.FromResult((TResponse)Response!);
        }

        public Task PublishAsync(INotification notification, CancellationToken cancellationToken = default)
        {
            Notification = notification;

            return Task.CompletedTask;
        }
    }
}
