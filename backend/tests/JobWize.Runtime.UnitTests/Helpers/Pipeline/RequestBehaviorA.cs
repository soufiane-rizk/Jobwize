using JobWize.Runtime.Contracts.Pipelines;
using JobWize.Runtime.Contracts.Requests;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Runtime.UnitTests.Helpers.Pipeline
{
    public sealed class RequestBehaviorA<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly PipelineExecutionRecorder _recorder;

        public RequestBehaviorA(PipelineExecutionRecorder recorder)
        {
            _recorder = recorder;
        }

        public async Task<TResponse> HandleAsync(ExecutionContext<TRequest, TResponse> context, RequestExecutionDelegate<TResponse> next)
        {
            _recorder.Events.Add("RequestA.Before");

            TResponse response = await next();

            _recorder.Events.Add("RequestA.After");

            return response;
        }
    }
}
