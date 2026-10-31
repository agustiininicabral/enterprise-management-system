using EnterpriseManagement.Api.Contracts;
using EnterpriseManagement.Api.Data;
using EnterpriseManagement.Api.Domain;
using Microsoft.EntityFrameworkCore;
namespace EnterpriseManagement.Api.Services;
public sealed class TaskService(AppDbContext db) : ITaskService
{
    public async Task<IReadOnlyList<TaskResponse>> GetAllAsync(Guid userId, CancellationToken ct) =>
        await db.Tasks.AsNoTracking().Where(x => x.UserId == userId).OrderByDescending(x => x.CreatedAt).Select(x => Map(x)).ToListAsync(ct);
    public async Task<TaskResponse?> GetByIdAsync(Guid userId, Guid id, CancellationToken ct)
    {
        var item = await db.Tasks.AsNoTracking().FirstOrDefaultAsync(x => x.UserId == userId && x.Id == id, ct);
        return item is null ? null : Map(item);
    }
    public async Task<TaskResponse> CreateAsync(Guid userId, CreateTaskRequest request, CancellationToken ct)
    {
        var item = new WorkTask { UserId = userId, Title = request.Title.Trim(), Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim() };
        db.Tasks.Add(item); await db.SaveChangesAsync(ct); return Map(item);
    }
    public async Task<TaskResponse?> UpdateAsync(Guid userId, Guid id, UpdateTaskRequest request, CancellationToken ct)
    {
        var item = await db.Tasks.FirstOrDefaultAsync(x => x.UserId == userId && x.Id == id, ct);
        if (item is null) return null;
        item.Title = request.Title.Trim(); item.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        item.Status = request.Status; item.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct); return Map(item);
    }
    public async Task<bool> DeleteAsync(Guid userId, Guid id, CancellationToken ct)
    {
        var item = await db.Tasks.FirstOrDefaultAsync(x => x.UserId == userId && x.Id == id, ct);
        if (item is null) return false;
        db.Tasks.Remove(item); await db.SaveChangesAsync(ct); return true;
    }
    private static TaskResponse Map(WorkTask x) => new(x.Id, x.Title, x.Description, x.Status, x.CreatedAt, x.UpdatedAt);
}
