using EnterpriseManagement.Api.Domain;
namespace EnterpriseManagement.Api.Contracts;
public sealed record CreateTaskRequest(string Title, string? Description);
public sealed record UpdateTaskRequest(string Title, string? Description, WorkTaskStatus Status);
public sealed record TaskResponse(Guid Id, string Title, string? Description, WorkTaskStatus Status, DateTime CreatedAt, DateTime UpdatedAt);
