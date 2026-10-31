using EnterpriseManagement.Api.Contracts;
using EnterpriseManagement.Api.Data;
using EnterpriseManagement.Api.Domain;
using EnterpriseManagement.Api.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController(
    AppDbContext db,
    IPasswordHasher<User> hasher,
    ITokenService tokens) : ControllerBase
{
    [HttpPost("register")]
    [EnableRateLimiting("auth-register")]
    public async Task<IActionResult> Register(RegisterRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest(new { error = "Email and password are required." });

        var email = request.Email.Trim().ToLowerInvariant();
        if (!IsValidEmail(email))
            return BadRequest(new { error = "A valid email is required." });
        if (!IsStrongPassword(request.Password))
            return BadRequest(new { error = "Password must be at least 12 characters and include uppercase, lowercase and a number." });

        if (await db.Users.AnyAsync(user => user.Email == email, ct))
            return Conflict(new { error = "An account with this email already exists." });

        var user = new User { Email = email };
        user.PasswordHash = hasher.HashPassword(user, request.Password);
        db.Users.Add(user);

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            if (await EmailAlreadyExistsAsync(email, ct))
                return Conflict(new { error = "An account with this email already exists." });

            throw;
        }

        var (token, expires) = tokens.CreateToken(user);
        return StatusCode(StatusCodes.Status201Created, new AuthResponse(token, expires, user.Email));
    }

    [HttpPost("login")]
    [EnableRateLimiting("auth-login")]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest(new { error = "Email and password are required." });

        var email = request.Email.Trim().ToLowerInvariant();
        var user = await db.Users.SingleOrDefaultAsync(item => item.Email == email, ct);
        if (user is null ||
            hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password) == PasswordVerificationResult.Failed)
        {
            return Unauthorized(new { error = "Invalid email or password." });
        }

        var (token, expires) = tokens.CreateToken(user);
        return Ok(new AuthResponse(token, expires, user.Email));
    }

    private Task<bool> EmailAlreadyExistsAsync(string email, CancellationToken ct) =>
        db.Users.AsNoTracking().AnyAsync(user => user.Email == email, ct);

    private static bool IsStrongPassword(string password) =>
        password.Length >= 12 &&
        password.Any(char.IsUpper) &&
        password.Any(char.IsLower) &&
        password.Any(char.IsDigit);

    private static bool IsValidEmail(string email) =>
        email.Length <= 256 &&
        System.Net.Mail.MailAddress.TryCreate(email, out var parsed) &&
        string.Equals(parsed.Address, email, StringComparison.OrdinalIgnoreCase);
}
