using EnterpriseManagement.Api.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseManagement.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin")]
public sealed class AdminController(AppDbContext db) : ControllerBase
{
    [HttpGet("users")]
    public async Task<IActionResult> GetUsers(CancellationToken ct)
    {
        var users = await db.Users.AsNoTracking()
            .OrderBy(user => user.Email)
            .Select(user => new { user.Id, user.Email, user.Role, user.CreatedAt })
            .ToListAsync(ct);
        return Ok(users);
    }
}
