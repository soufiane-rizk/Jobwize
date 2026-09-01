using JobWize.Runtime.Contracts.Pipelines;
using JobWize.Shared.Application.Results;
using JobWize.Shared.Errors;
using JobWize.Shared.Runtime.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Runtime.UnitTests.Helpers.Pipeline
{
    public sealed class BlockingCommandBehavior<TCommand, TResult> : IPipelineBehavior<TCommand, Result<TResult>>
    where TCommand : ICommand<TResult>
    {
        private readonly PipelineExecutionRecorder _recorder;

        public BlockingCommandBehavior(PipelineExecutionRecorder recorder)
        {
            _recorder = recorder;
        }

        public Task<Result<TResult>> HandleAsync(
            ExecutionContext<TCommand, Result<TResult>> context,
            RequestExecutionDelegate<Result<TResult>> next)
        {
            _recorder.Events.Add("Command.Before");

            return Task.FromResult(
                Result<TResult>.Failure(
                    new Error(
                        "Pipeline.Blocked",
                        "The request was blocked by the pipeline.",
                        ErrorType.Validation)));
        }
    }
}
