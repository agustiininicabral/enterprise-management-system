using EnterpriseManagement.Api.Contracts;
using EnterpriseManagement.Api.Data;
using EnterpriseManagement.Api.Domain;
using EnterpriseManagement.Api.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EnterpriseManagement.Tests;

public sealed class TaskServiceTests
{
    private static (AppDbContext Db, TaskService Service) CreateService()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var db = new AppDbContext(options);
        return (db, new TaskService(db));
    }

    [Fact]
    public async Task CreateAsync_PersistsTrimmedTaskForOwner()
    {
        var (db, service) = CreateService();
        await using (db)
        {
            var owner = Guid.NewGuid();
            var created = await service.CreateAsync(owner, new CreateTaskRequest("  Prepare release  ", "  QA checklist  "), CancellationToken.None);

            Assert.NotEqual(Guid.Empty, created.Id);
            Assert.Equal("Prepare release", created.Title);
            Assert.Equal("QA checklist", created.Description);
            Assert.Equal(owner, (await db.Tasks.SingleAsync()).UserId);
        }
    }

    [Fact]
    public async Task GetByIdAsync_DoesNotExposeAnotherUsersTask()
    {
        var (db, service) = CreateService();
        await using (db)
        {
            var owner = Guid.NewGuid();
            var created = await service.CreateAsync(owner, new CreateTaskRequest("Private task", null), CancellationToken.None);

            Assert.NotNull(await service.GetByIdAsync(owner, created.Id, CancellationToken.None));
            Assert.Null(await service.GetByIdAsync(Guid.NewGuid(), created.Id, CancellationToken.None));
        }
    }

    [Fact]
    public async Task UpdateAsync_ChangesStatusAndTimestamp()
    {
        var (db, service) = CreateService();
        await using (db)
        {
            var owner = Guid.NewGuid();
            var created = await service.CreateAsync(owner, new CreateTaskRequest("Ship feature", null), CancellationToken.None);
            var updated = await service.UpdateAsync(owner, created.Id,
                new UpdateTaskRequest("Ship feature", "Ready for review", WorkTaskStatus.Done), CancellationToken.None);

            Assert.NotNull(updated);
            Assert.Equal(WorkTaskStatus.Done, updated.Status);
            Assert.Equal("Ready for review", updated.Description);
        }
    }

    [Fact]
    public async Task DeleteAsync_RemovesOnlyOwnedTask()
    {
        var (db, service) = CreateService();
        await using (db)
        {
            var owner = Guid.NewGuid();
            var created = await service.CreateAsync(owner, new CreateTaskRequest("Remove me", null), CancellationToken.None);

            Assert.False(await service.DeleteAsync(Guid.NewGuid(), created.Id, CancellationToken.None));
            Assert.True(await service.DeleteAsync(owner, created.Id, CancellationToken.None));
            Assert.Empty(await db.Tasks.ToListAsync());
        }
    }
}
