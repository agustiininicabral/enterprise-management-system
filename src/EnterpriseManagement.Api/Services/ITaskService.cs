using EnterpriseManagement.Api.Contracts;
namespace EnterpriseManagement.Api.Services;
public interface ITaskService
{
    Task<IReadOnlyList<TaskResponse>> GetAllAsync(Guid userId, CancellationToken ct);
    Task<TaskResponse?> GetByIdAsync(Guid userId, Guid id, CancellationToken ct);
    Task<TaskResponse> CreateAsync(Guid userId, CreateTaskRequest request, CancellationToken ct);
    Task<TaskResponse?> UpdateAsync(Guid userId, Guid id, UpdateTaskRequest request, CancellationToken ct);
    Task<bool> DeleteAsync(Guid userId, Guid id, CancellationToken ct);
}
