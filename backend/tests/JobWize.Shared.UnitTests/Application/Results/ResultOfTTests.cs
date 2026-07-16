using FluentAssertions;
using JobWize.Shared.Application.Results;
using Xunit;

namespace JobWize.Shared.UnitTests.Application.Results;

public class ResultOfTTests
{
    [Fact]
    public void Success_ShouldCreateSuccessfulResult()
    {
        // Arrange

        // Act
        Result<int> result = Result<int>.Success(42);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Value.Should().Be(42);
        result.Error.Should().Be(SharedErrors.None);
    }

    [Fact]
    public void Failure_ShouldCreateFailedResult()
    {
        // Arrange
        Error error = new("Test.Error", "An error occurred.", ErrorType.Failure);

        // Act
        Result<int> result = Result<int>.Failure(error);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void Value_ShouldThrow_WhenResultIsFailure()
    {
        // Arrange
        Error error = new("Test.Error", "An error occurred.", ErrorType.Failure);
        Result<int> result = Result<int>.Failure(error);

        // Act
        Action action = () => _ = result.Value;

        // Assert
        action.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("A failed result has no value.");
    }

    [Fact]
    public void Success_ShouldAllowNullReferenceValue()
    {
        // Arrange

        // Act
        Result<string?> result = Result<string?>.Success(null);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Value.Should().BeNull();
        result.Error.Should().Be(SharedErrors.None);
    }
}