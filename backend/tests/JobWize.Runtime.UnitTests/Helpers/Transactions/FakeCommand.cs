using JobWize.Shared.Runtime.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Runtime.UnitTests.Helpers.Transactions
{
    internal sealed record FakeCommand : ICommand<Guid>;
}
