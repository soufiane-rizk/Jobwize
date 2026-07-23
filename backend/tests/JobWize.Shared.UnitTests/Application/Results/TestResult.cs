using JobWize.Shared.Application.Results;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Shared.UnitTests.Application.Results
{
    internal sealed class TestResult : Result
    {
        public TestResult(bool isSuccess, Error error)
            : base(isSuccess, error)
        {
        }
    }
}
