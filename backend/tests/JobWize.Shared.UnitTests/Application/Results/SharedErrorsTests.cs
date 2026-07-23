using FluentAssertions;
using JobWize.Shared.Application.Results;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Shared.UnitTests.Application.Results
{
    public class SharedErrorsTests
    {
        [Fact]
        public void None_ShouldRepresentNoError()
        {
            // Arrange

            // Act
            Error error = SharedErrors.None;

            // Assert
            error.Code.Should().BeEmpty();
            error.Message.Should().BeEmpty();
            error.Type.Should().Be(ErrorType.Failure);
        }
    }
}
