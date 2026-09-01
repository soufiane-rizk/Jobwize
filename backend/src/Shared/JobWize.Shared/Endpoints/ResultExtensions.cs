using JobWize.Shared.Application.Results;
using JobWize.Shared.Errors;
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

            return ToFailureResult(result.Error, result.Confirmations);
        }

        public static IResult ToApiResult<T>(this Result<T> result)
        {
            if (result.IsSuccess)
            {
                return Results.Ok(result.Value);
            }

            return ToFailureResult(result.Error, result.Confirmations);
        }

        private static IResult ToFailureResult(Error error, IReadOnlyList<Confirmation> confirmations)
        {
            int statusCode = error.Type switch
            {
                ErrorType.Validation => StatusCodes.Status400BadRequest,
                ErrorType.Conflict => StatusCodes.Status409Conflict,
                ErrorType.NotFound => StatusCodes.Status404NotFound,
                ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
                ErrorType.Forbidden => StatusCodes.Status403Forbidden,
                ErrorType.ConfirmationRequired => StatusCodes.Status409Conflict,
                _ => StatusCodes.Status500InternalServerError
            };

            return Problem(error, statusCode, confirmations);
        }

        private static IResult Problem(Error error, int statusCode, IReadOnlyList<Confirmation> confirmations)
        {
            return Results.Json(
                CreateProblem(error, statusCode, confirmations),
                contentType: "application/problem+json",
                statusCode: statusCode);
        }

        private static ProblemDetails CreateProblem(Error error, int statusCode, IReadOnlyList<Confirmation> confirmations)
        {
            ProblemDetails problem = new()
            {
                Title = GetTitle(error.Type),
                Detail = error.Message,
                Status = statusCode
            };

            problem.Extensions["code"] = error.Code;
            if (confirmations.Count > 0) problem.Extensions["confirmations"] = confirmations;

            if (error.Details is not null && error.Details.Count > 0)
            {
                problem.Extensions["errors"] = error.Details
                    .GroupBy(x => x.Field)
                    .ToDictionary(
                        group => group.Key,
                        group => group.Select(x => x.Message).ToArray());
            }

            return problem;
        }

        private static string GetTitle(ErrorType type) =>
            type switch
            {
                ErrorType.Validation => "Validation failed",
                ErrorType.Conflict => "Conflict",
                ErrorType.NotFound => "Resource not found",
                ErrorType.Unauthorized => "Unauthorized",
                ErrorType.Forbidden => "Forbidden",
                ErrorType.ConfirmationRequired => "Confirmation required",
                _ => "Unexpected error"
            };
    }
}
