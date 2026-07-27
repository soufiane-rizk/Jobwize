using JobWize.Runtime.Contracts.Pipelines;
using JobWize.Runtime.Contracts.Requests;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Runtime.UnitTests.Helpers.Pipeline
{
    public sealed class RequestBehaviorC<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly PipelineExecutionRecorder _recorder;

        public RequestBehaviorC(PipelineExecutionRecorder recorder)
        {
            _recorder = recorder;
        }

        public async Task<TResponse> HandleAsync(ExecutionContext<TRequest, TResponse> context, RequestExecutionDelegate<TResponse> next)
        {
            _recorder.Events.Add("RequestC.Before");

            TResponse response = await next();

            _recorder.Events.Add("RequestC.After");

            return response;
        }
    }
}
