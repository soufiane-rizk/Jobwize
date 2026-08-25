using JobWize.Frontend.Shared.Results;
using Microsoft.AspNetCore.Components;

namespace JobWize.Frontend.Shared.Forms
{
    public abstract class ResultFormComponentBase<TResponse> : ComponentBase
    {
        protected Result<TResponse>? SubmissionResult { get; private set; }

        protected int SubmissionVersion { get; private set; }

        protected bool IsSubmitting { get; set; }

        protected void SetSubmissionResult(Result<TResponse> result)
        {
            SubmissionResult = result;
            SubmissionVersion++;
        }

        protected void ClearSubmissionResult()
        {
            SubmissionResult = null;
            SubmissionVersion++;
        }

        protected string? GetServerFieldError(string fieldName)
        {
            if (SubmissionResult?.Error?.ValidationErrors?.TryGetValue(fieldName, out string[]? errors) != true)
            {
                return null;
            }

            return errors?.FirstOrDefault();
        }
    }
}
