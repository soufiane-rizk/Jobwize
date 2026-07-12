using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Shared.Exceptions
{
    public abstract class BusinessRuleException : Exception
    {
        protected BusinessRuleException(string message)
            : base(message)
        {
        }
    }
}
