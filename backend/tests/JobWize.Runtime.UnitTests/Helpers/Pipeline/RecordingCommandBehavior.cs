using JobWize.Runtime.Contracts.Pipelines;
using JobWize.Shared.Application.Results;
using JobWize.Shared.Runtime.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Runtime.UnitTests.Helpers.Pipeline
{
    public sealed class RecordingCommandBehavior<TCommand, TResult> : IPipelineBehavior<TCommand, Result<TResult>>
        where TCommand : ICommand<TResult>
    {
        private readonly PipelineExecutionRecorder _recorder;

        public RecordingCommandBehavior(PipelineExecutionRecorder recorder)
        {
            _recorder = recorder;
        }

        public async Task<Result<TResult>> HandleAsync(ExecutionContext<TCommand, Result<TResult>> context, RequestExecutionDelegate<Result<TResult>> next)
        {
            _recorder.Events.Add("Command.Before");

            Result<TResult> response = await next();

            _recorder.Events.Add("Command.After");

            return response;
        }
    }
}
