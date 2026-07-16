using FluentAssertions;
using JobWize.Shared.Application.Results;
using Xunit;

namespace JobWize.Shared.UnitTests.Application.Results;

public class ResultTests
{
    [Fact]
    public void Success_ShouldCreateSuccessfulResult()
    {
        // Arrange

        // Act
        Result result = Result.Success();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Error.Should().Be(SharedErrors.None);
    }

    [Fact]
    public void Failure_ShouldCreateFailedResult()
    {
        // Arrange
        Error error = new("Test.Error", "An error occurred.", ErrorType.Failure);

        // Act
        Result result = Result.Failure(error);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenSuccessfulResultContainsError()
    {
        // Arrange
        Error error = new("Test.Error", "An error occurred.", ErrorType.Failure);

        // Act
        Action action = () => new TestResult(true, error);

        // Assert
        action.Should()
            .Throw<ArgumentException>()
            .WithMessage("Successful results cannot contain an error.");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenFailedResultContainsNoError()
    {
        // Arrange

        // Act
        Action action = () => new TestResult(false, SharedErrors.None);

        // Assert
        action.Should()
            .Throw<ArgumentException>()
            .WithMessage("Failed results must contain an error.");
    }
}