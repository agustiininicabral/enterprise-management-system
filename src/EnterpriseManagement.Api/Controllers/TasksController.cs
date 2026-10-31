using System.Security.Claims;
using EnterpriseManagement.Api.Contracts;
using EnterpriseManagement.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace EnterpriseManagement.Api.Controllers;
[ApiController, Authorize, Route("api/[controller]")]
public sealed class TasksController(ITaskService service) : ControllerBase
{
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct) => Ok(await service.GetAllAsync(UserId, ct));
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var item = await service.GetByIdAsync(UserId, id, ct);
        return item is null ? NotFound() : Ok(item);
    }
    [HttpPost]
    public async Task<IActionResult> Create(CreateTaskRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Title) || request.Title.Trim().Length > 120)
            return BadRequest(new { error = "Title is required and must be at most 120 characters." });
        if (request.Description?.Length > 2000) return BadRequest(new { error = "Description must be at most 2000 characters." });
        var created = await service.CreateAsync(UserId, request, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateTaskRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Title) || request.Title.Trim().Length > 120 ||
            request.Description?.Length > 2000 || !Enum.IsDefined(request.Status))
            return BadRequest(new { error = "Invalid task data." });
        var updated = await service.UpdateAsync(UserId, id, request, ct);
        return updated is null ? NotFound() : Ok(updated);
    }
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct) =>
        await service.DeleteAsync(UserId, id, ct) ? NoContent() : NotFound();
}
