using JobWize.Shared.Application.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Shared.Endpoints
{
    public static class ResultExtensions
    {
        public static IResult ToApiResult(this Result result)
        {
            if (result.IsSuccess)
            {
                return Results.NoContent();
            }

            return result.Error.Type switch
            {
                ErrorType.Validation => Results.BadRequest(CreateProblem(result.Error)),
                ErrorType.Conflict => Results.Conflict(CreateProblem(result.Error)),
                ErrorType.NotFound => Results.NotFound(CreateProblem(result.Error)),
                ErrorType.Unauthorized => Results.Unauthorized(),
                ErrorType.Forbidden => Results.Forbid(),
                _ => Results.Problem(
                    title: result.Error.Code,
                    detail: result.Error.Message)
            };
        }

        public static IResult ToApiResult<T>(this Result<T> result)
        {
            if (result.IsSuccess)
            {
                return Results.Ok(result.Value);
            }

            return result.Error.Type switch
            {
                ErrorType.Validation => Results.BadRequest(CreateProblem(result.Error)),
                ErrorType.Conflict => Results.Conflict(CreateProblem(result.Error)),
                ErrorType.NotFound => Results.NotFound(CreateProblem(result.Error)),
                ErrorType.Unauthorized => Results.Unauthorized(),
                ErrorType.Forbidden => Results.Forbid(),
                _ => Results.Problem(
                    title: result.Error.Code,
                    detail: result.Error.Message)
            };
        }

        private static ProblemDetails CreateProblem(Error error)
        {
            return new ProblemDetails
            {
                Title = error.Code,
                Detail = error.Message
            };
        }
    }
}
