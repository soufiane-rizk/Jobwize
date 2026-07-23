using JobWize.Runtime.Contracts.Notifications;
using JobWize.Runtime.Contracts.Requests;
using JobWize.Runtime.Discovery;
using JobWize.Runtime.Execution;
using JobWize.Shared.Application.Results;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Runtime.UnitTests.Helpers
{
    internal class FakeModuleRuntime : IModuleRuntime
    {
        public bool SendCalled { get; private set; }
        public bool PublishCalled { get; private set; }

        public string Name => "Fake";

        public IEnumerable<Type> DispatchableTypes => [];

        public object? Request { get; private set; }

        public object? ModuleQuery { get; private set; }

        public object? Notification { get; private set; }

        public IServiceProvider? ServiceProvider { get; private set; }

        public object? Response { get; set; }

        public IEnumerable<Type> NotificationTypes => [];

        public Task<TResponse> SendAsync<TResponse>(IServiceProvider serviceProvider, IRequest<TResponse> request, CancellationToken cancellationToken)
        {
            SendCalled = true;

            Request = request;
            ServiceProvider = serviceProvider;

            return Task.FromResult((TResponse)Response!);
        }

        public Task<TResponse> SendAsync<TResponse>(IServiceProvider serviceProvider, IModuleQuery<TResponse> query, CancellationToken cancellationToken)
        {
            SendCalled = true;

            ModuleQuery = query;
            ServiceProvider = serviceProvider;

            return Task.FromResult((TResponse)Response!);
        }

        public virtual Task PublishAsync(IServiceProvider serviceProvider, INotification notification, CancellationToken cancellationToken = default)
        {
            PublishCalled = true;

            Notification = notification;
            ServiceProvider = serviceProvider;

            return Task.CompletedTask;
        }
    }
}
