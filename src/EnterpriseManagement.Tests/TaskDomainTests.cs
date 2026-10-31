using EnterpriseManagement.Api.Domain;
using Xunit;
using System.Text.Json;
using System.Text.Json.Serialization;
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
    [Fact]
    public void TaskStatus_SerializesAsStringForFrontendContract()
    {
        var options = new JsonSerializerOptions();
        options.Converters.Add(new JsonStringEnumConverter());
        Assert.Equal("\\"InProgress\\"", JsonSerializer.Serialize(WorkTaskStatus.InProgress, options));
        Assert.Equal(WorkTaskStatus.Done, JsonSerializer.Deserialize<WorkTaskStatus>("\\"Done\\"", options));
    }

    [Theory]
    [InlineData(WorkTaskStatus.Todo, "Todo")]
    [InlineData(WorkTaskStatus.InProgress, "InProgress")]
    [InlineData(WorkTaskStatus.Done, "Done")]
    public void Status_HasExpectedValues(WorkTaskStatus status, string expected) =>
        Assert.Equal(expected, status.ToString());
}
