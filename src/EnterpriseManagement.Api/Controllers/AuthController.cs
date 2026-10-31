using EnterpriseManagement.Api.Contracts;
using EnterpriseManagement.Api.Data;
using EnterpriseManagement.Api.Domain;
using EnterpriseManagement.Api.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace EnterpriseManagement.Api.Controllers;
[ApiController]
[Route("api/[controller]")]
public sealed class AuthController(AppDbContext db, IPasswordHasher<User> hasher, ITokenService tokens) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest(new { error = "Email and password are required." });
        var email = request.Email.Trim().ToLowerInvariant();
        if (!IsValidEmail(email)) return BadRequest(new { error = "A valid email is required." });
        if (request.Password.Length < 10) return BadRequest(new { error = "Password must contain at least 10 characters." });
        if (await db.Users.AnyAsync(x => x.Email == email, ct)) return Conflict(new { error = "An account with this email already exists." });
        var user = new User { Email = email };
        user.PasswordHash = hasher.HashPassword(user, request.Password);
        db.Users.Add(user); await db.SaveChangesAsync(ct);
        var (token, expires) = tokens.CreateToken(user);
        return StatusCode(StatusCodes.Status201Created, new AuthResponse(token, expires, user.Email));
    }
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest(new { error = "Email and password are required." });
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await db.Users.SingleOrDefaultAsync(x => x.Email == email, ct);
        if (user is null || hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password) == PasswordVerificationResult.Failed)
            return Unauthorized(new { error = "Invalid email or password." });
        var (token, expires) = tokens.CreateToken(user);
        return Ok(new AuthResponse(token, expires, user.Email));
    }
    private static bool IsValidEmail(string email) => email.Length <= 256 && email.Contains('@') && email.LastIndexOf('.') > email.IndexOf('@') + 1;
}
