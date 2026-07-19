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
        context.IsActive.Should().BeTrue();
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
        context.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Collect_Should_Throw_When_Context_Is_Not_Active()
    {
        // Arrange
        NotificationContext context = new();

        ItemCreated notification = new(Guid.NewGuid());

        // Act
        Action action = () => context.Collect(notification);

        // Assert
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot collect*");
    }

    [Fact]
    public void Complete_Should_Return_Collected_Notifications()
    {
        // Arrange
        NotificationContext context = new();

        ItemCreated first = new(Guid.NewGuid());
        ItemCreated second = new(Guid.NewGuid());

        context.Begin();

        context.Collect(first);
        context.Collect(second);

        // Act
        IReadOnlyCollection<INotification> notifications = context.Complete();

        // Assert
        notifications.Should().HaveCount(2);
        notifications.Should().Contain(first);
        notifications.Should().Contain(second);

        context.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Complete_Should_Throw_When_Context_Is_Not_Active()
    {
        // Arrange
        NotificationContext context = new();

        // Act
        Action action = () => context.Complete();

        // Assert
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("*No active transaction*");
    }

    [Fact]
    public void Abort_Should_Clear_Context()
    {
        // Arrange
        NotificationContext context = new();

        context.Begin();

        context.Collect(new ItemCreated(Guid.NewGuid()));

        // Act
        context.Abort();

        // Assert
        context.IsActive.Should().BeFalse();

        bool isRoot = context.Begin();

        isRoot.Should().BeTrue();

        IReadOnlyCollection<INotification> notifications = context.Complete();

        notifications.Should().BeEmpty();
    }


    [Fact]
    public void Complete_Should_Preserve_Notification_Order()
    {
        // Arrange
        NotificationContext context = new();

        ItemCreated first = new(Guid.NewGuid());
        ItemCreated second = new(Guid.NewGuid());
        ItemCreated third = new(Guid.NewGuid());

        context.Begin();

        context.Collect(first);
        context.Collect(second);
        context.Collect(third);

        // Act
        IReadOnlyCollection<INotification> notifications = context.Complete();

        // Assert
        notifications.Should().Equal(first, second, third);
    }
}