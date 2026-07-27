using JobWize.Runtime.Contracts.Pipelines;
using JobWize.Shared.Application.Results;
using JobWize.Shared.Runtime.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Runtime.UnitTests.Helpers.Pipeline
{
    public sealed class RecordingQueryBehavior<TQuery, TResult> : IPipelineBehavior<TQuery, Result<TResult>>
        where TQuery : IQuery<TResult>
    {
        private readonly PipelineExecutionRecorder _recorder;

        public RecordingQueryBehavior(PipelineExecutionRecorder recorder)
        {
            _recorder = recorder;
        }

        public async Task<Result<TResult>> HandleAsync(ExecutionContext<TQuery, Result<TResult>> context, RequestExecutionDelegate<Result<TResult>> next)
        {
            _recorder.Events.Add("Query.Before");

            Result<TResult> response = await next();

            _recorder.Events.Add("Query.After");

            return response;
        }
    }
}
