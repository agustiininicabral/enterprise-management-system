using EnterpriseManagement.Api.Domain;
using Xunit;
namespace EnterpriseManagement.Tests;
public sealed class TaskDomainTests
{
    [Fact]
    public void NewTask_HasGeneratedId_AndDefaultsToTodo()
    {
        var task = new WorkTask();
        Assert.NotEqual(Guid.Empty, task.Id);
        Assert.Equal(WorkTaskStatus.Todo, task.Status);
    }
    [Theory]
    [InlineData(WorkTaskStatus.Todo, "Todo")]
    [InlineData(WorkTaskStatus.InProgress, "InProgress")]
    [InlineData(WorkTaskStatus.Done, "Done")]
    public void Status_HasExpectedValues(WorkTaskStatus status, string expected) =>
        Assert.Equal(expected, status.ToString());
}
