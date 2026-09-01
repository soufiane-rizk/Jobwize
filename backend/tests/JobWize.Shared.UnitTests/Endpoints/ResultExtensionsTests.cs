using FluentAssertions;
using JobWize.Shared.Application.Results;
using JobWize.Shared.Errors;
using JobWize.Shared.Endpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace JobWize.Shared.UnitTests.Endpoints
{
    public sealed class ResultExtensionsTests
    {
        [Theory]
        [InlineData(ErrorType.Validation, StatusCodes.Status400BadRequest, "Validation failed")]
        [InlineData(ErrorType.Conflict, StatusCodes.Status409Conflict, "Conflict")]
        [InlineData(ErrorType.NotFound, StatusCodes.Status404NotFound, "Resource not found")]
        [InlineData(ErrorType.Unauthorized, StatusCodes.Status401Unauthorized, "Unauthorized")]
        [InlineData(ErrorType.Forbidden, StatusCodes.Status403Forbidden, "Forbidden")]
        [InlineData(ErrorType.Failure, StatusCodes.Status500InternalServerError, "Unexpected error")]
        public async Task ToApiResult_Should_Map_Error_Type_To_Standard_Problem_Details(
            ErrorType errorType,
            int expectedStatusCode,
            string expectedTitle)
        {
            // Arrange
            Error error = new("Test.Error", "Test error message.", errorType);

            DefaultHttpContext context = CreateContext();

            IResult apiResult = Result.Failure(error).ToApiResult();

            // Act
            await apiResult.ExecuteAsync(context);

            context.Response.Body.Position = 0;

            JsonDocument response = await JsonDocument.ParseAsync(context.Response.Body);

            // Assert
            context.Response.StatusCode.Should().Be(expectedStatusCode);
            response.RootElement.GetProperty("title").GetString().Should().Be(expectedTitle);
            response.RootElement.GetProperty("detail").GetString().Should().Be(error.Message);
            response.RootElement.GetProperty("status").GetInt32().Should().Be(expectedStatusCode);
            response.RootElement.GetProperty("code").GetString().Should().Be(error.Code);
        }

        [Fact]
        public async Task ToApiResult_Should_Return_Standard_Problem_Details_For_Unexpected_Error()
        {
            // Arrange
            DefaultHttpContext context = CreateContext();

            IResult apiResult = Result.Failure(SharedErrors.Unexpected)
                .ToApiResult();

            // Act
            await apiResult.ExecuteAsync(context);

            context.Response.Body.Position = 0;

            JsonDocument response = await JsonDocument.ParseAsync(context.Response.Body);

            // Assert
            context.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);

            response.RootElement.GetProperty("title").GetString()
                .Should().Be("Unexpected error");

            response.RootElement.GetProperty("detail").GetString()
                .Should().Be(SharedErrors.Unexpected.Message);

            response.RootElement.GetProperty("status").GetInt32()
                .Should().Be(StatusCodes.Status500InternalServerError);

            response.RootElement.GetProperty("code").GetString()
                .Should().Be(SharedErrors.Unexpected.Code);
        }

        private static DefaultHttpContext CreateContext()
        {
            DefaultHttpContext context = new();
            context.Response.Body = new MemoryStream();
            context.RequestServices = new ServiceCollection()
                .AddOptions()
                .AddLogging()
                .BuildServiceProvider();

            return context;
        }
    }
}
