using FluentAssertions;
using JobWize.Runtime.Transactions;
using JobWize.Runtime.UnitTests.Helpers.Transactions;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Runtime.UnitTests.Registration
{
    internal sealed class MonolithTransactionManagerTests
    {
        [Fact]
        public async Task BeginAsync_Should_Begin_All_Transaction_Contexts()
        {
            // Arrange
            RecordingTransactionContext context1 = new("Context1");
            RecordingTransactionContext context2 = new("Context2");

            MonolithTransactionManager manager =
                TransactionTestHelper.CreateManager(context1, context2);

            // Act
            await manager.BeginAsync();

            // Assert
            context1.Calls.Should().Equal("Context1.Begin");
            context2.Calls.Should().Equal("Context2.Begin");
        }

        [Fact]
        public async Task PersistChangesAsync_Should_Persist_All_Transaction_Contexts()
        {
            // Arrange
            RecordingTransactionContext context1 = new("Context1");
            RecordingTransactionContext context2 = new("Context2");

            MonolithTransactionManager manager =
                TransactionTestHelper.CreateManager(context1, context2);

            // Act
            await manager.PersistChangesAsync();

            // Assert
            context1.Calls.Should().Equal("Context1.Persist");
            context2.Calls.Should().Equal("Context2.Persist");
        }

        [Fact]
        public async Task CommitAsync_Should_Commit_All_Transaction_Contexts()
        {
            // Arrange
            RecordingTransactionContext context1 = new("Context1");
            RecordingTransactionContext context2 = new("Context2");

            MonolithTransactionManager manager =
                TransactionTestHelper.CreateManager(context1, context2);

            // Act
            await manager.CommitAsync();

            // Assert
            context1.Calls.Should().Equal("Context1.Commit");
            context2.Calls.Should().Equal("Context2.Commit");
        }

        [Fact]
        public async Task RollbackAsync_Should_Rollback_All_Transaction_Contexts()
        {
            // Arrange
            RecordingTransactionContext context1 = new("Context1");
            RecordingTransactionContext context2 = new("Context2");

            MonolithTransactionManager manager =
                TransactionTestHelper.CreateManager(context1, context2);

            // Act
            await manager.RollbackAsync();

            // Assert
            context1.Calls.Should().Equal("Context1.Rollback");
            context2.Calls.Should().Equal("Context2.Rollback");
        }

        [Fact]
        public async Task Methods_Should_Not_Throw_When_No_Transaction_Contexts_Are_Registered()
        {
            // Arrange
            MonolithTransactionManager manager =
                TransactionTestHelper.CreateManager();

            // Act
            Func<Task> act = async () =>
            {
                await manager.BeginAsync();
                await manager.PersistChangesAsync();
                await manager.CommitAsync();
                await manager.RollbackAsync();
            };

            // Assert
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task Methods_Should_Execute_Transaction_Contexts_In_Registration_Order()
        {
            // Arrange
            RecordingTransactionContext context1 = new("Context1");
            RecordingTransactionContext context2 = new("Context2");

            MonolithTransactionManager manager =
                TransactionTestHelper.CreateManager(context1, context2);

            // Act
            await manager.BeginAsync();
            await manager.PersistChangesAsync();
            await manager.CommitAsync();

            // Assert
            context1.Calls.Should().Equal(
                "Context1.Begin",
                "Context1.Persist",
                "Context1.Commit");

            context2.Calls.Should().Equal(
                "Context2.Begin",
                "Context2.Persist",
                "Context2.Commit");
        }
    }
}
