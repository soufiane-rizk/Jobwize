using JobWize.Runtime.Contracts.Pipelines;
using JobWize.Runtime.Contracts.Requests;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Runtime.Pipelines
{
    internal static class PipelineTypeResolver
    {
        public static Type? TryClose(Type openBehavior, Type requestType)
        {
            Type requestGenericParameter = openBehavior.GetGenericArguments()[0];

            Type constraint = requestGenericParameter
                .GetGenericParameterConstraints()
                .Single();

            foreach (Type implemented in requestType.GetInterfaces())
            {
                if (!implemented.IsGenericType)
                    continue;

                if (implemented.GetGenericTypeDefinition() !=
                    constraint.GetGenericTypeDefinition())
                    continue;

                Type resultType = implemented.GetGenericArguments()[0];

                return openBehavior.MakeGenericType(
                    requestType,
                    resultType);
            }

            return null;
        }
    }
}
