using JobWize.Runtime.Transactions;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Runtime.UnitTests.Helpers.Transactions
{
    internal static class TransactionTestHelper
    {
        public static MonolithTransactionManager CreateManager(
            params RecordingTransactionContext[] contexts)
        {
            return new MonolithTransactionManager(contexts);
        }
    }
}
