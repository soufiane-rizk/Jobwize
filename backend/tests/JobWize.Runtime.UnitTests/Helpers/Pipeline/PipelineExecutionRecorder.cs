using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Runtime.UnitTests.Helpers.Pipeline
{

    public sealed class PipelineExecutionRecorder
    {
        public List<string> Events { get; } = [];
    }
}
