using FluentAssertions;
using JobWize.ModuleOne.Contracts;
using JobWize.ModuleOne.Features;
using JobWize.Runtime.Contracts.Notifications;
using JobWize.Runtime.Dispatching;
using JobWize.Runtime.Execution;

namespace JobWize.Runtime.UnitTests.Execution;

public sealed class NotificationContextTests
{
    [Fact]
    public void Begin_Should_Start_New_Context()
    {
        // Arrange
        NotificationContext context = new();

        // Act
        bool isRoot = context.Begin();

        // Assert
        isRoot.Should().BeTrue();
    }

    [Fact]
    public void Begin_Should_Return_False_When_Context_Is_Already_Active()
    {
        // Arrange
        NotificationContext context = new();

        context.Begin();

        // Act
        bool isRoot = context.Begin();

        // Assert
        isRoot.Should().BeFalse();
    }

    [Fact]
    public void Publish_Should_Throw_When_Context_Is_Not_Active()
    {
        // Arrange
        NotificationContext context = new();

        ItemCreated notification = new(Guid.NewGuid());

        // Act
        Action action = () => context.Publish(notification);

        // Assert
        action.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("*active notification workflow*");
    }

    [Fact]
    public void GetCurrentWave_Should_Return_Published_Notifications()
    {
        // Arrange
        NotificationContext context = new();

        ItemCreated first = new(Guid.NewGuid());
        ItemCreated second = new(Guid.NewGuid());

        context.Begin();

        context.Publish(first);
        context.Publish(second);

        // Act
        IReadOnlyCollection<INotification> notifications =
            context.GetCurrentWave();

        // Assert
        notifications.Should().Equal(first, second);
    }

    [Fact]
    public void Publish_Should_Queue_New_Notifications_In_Next_Wave()
    {
        // Arrange
        NotificationContext context = new();

        ItemCreated first = new(Guid.NewGuid());
        ItemCreated second = new(Guid.NewGuid());

        context.Begin();

        context.Publish(first);

        context.GetCurrentWave();

        // Act
        context.Publish(second);

        // Assert
        context.GetCurrentWave()
            .Should()
            .ContainSingle()
            .Which.Should()
            .BeSameAs(first);

        context.MoveToNextWave().Should().BeTrue();

        context.GetCurrentWave()
            .Should()
            .ContainSingle()
            .Which.Should()
            .BeSameAs(second);
    }

    [Fact]
    public void MoveToNextWave_Should_Return_False_When_No_Notifications_Are_Pending()
    {
        // Arrange
        NotificationContext context = new();

        context.Begin();

        context.Publish(new ItemCreated(Guid.NewGuid()));

        context.GetCurrentWave();

        // Act
        bool result = context.MoveToNextWave();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Abort_Should_Clear_Context()
    {
        // Arrange
        NotificationContext context = new();

        context.Begin();

        context.Publish(new ItemCreated(Guid.NewGuid()));

        context.GetCurrentWave();

        context.Complete();

        // Act
        bool isRoot = context.Begin();

        // Assert
        isRoot.Should().BeTrue();

        context.GetCurrentWave().Should().BeEmpty();
    }
}