using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using EnterpriseManagement.Api.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace EnterpriseManagement.Tests;

public sealed class ApiFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");

    public ApiFactory() => _connection.Open();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("Jwt:Issuer", "EnterpriseManagement.Tests");
        builder.UseSetting("Jwt:Audience", "EnterpriseManagement.Tests");
        builder.UseSetting("Jwt:Key", "integration-test-signing-key-with-at-least-32-bytes");
        builder.UseSetting("Jwt:ExpiresMinutes", "15");
        builder.UseSetting("Cors:Origins:0", "http://localhost:5173");

        builder.ConfigureAppConfiguration((_, configuration) =>
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Issuer"] = "EnterpriseManagement.Tests",
                ["Jwt:Audience"] = "EnterpriseManagement.Tests",
                ["Jwt:Key"] = "integration-test-signing-key-with-at-least-32-bytes",
                ["Jwt:ExpiresMinutes"] = "15",
                ["Cors:Origins:0"] = "http://localhost:5173"
            }));

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.AddDbContext<AppDbContext>(options => options.UseSqlite(_connection));
        });
    }

    public void InitializeDatabase()
    {
        using var scope = Services.CreateScope();
        scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.EnsureCreated();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing) _connection.Dispose();
    }
}

public sealed class ApiIntegrationTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task ProtectedTasksEndpoint_RejectsAnonymousRequests()
    {
        factory.InitializeDatabase();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/tasks");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task RegistrationLoginAndTaskCrud_WorkAgainstRelationalDatabase()
    {
        factory.InitializeDatabase();
        using var client = factory.CreateClient();

        var first = await Register(client, $"first-{Guid.NewGuid():N}@example.com");
        var second = await Register(client, $"second-{Guid.NewGuid():N}@example.com");

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", first.Token);
        var createResponse = await client.PostAsJsonAsync("/api/tasks", new { title = "Integration task", description = "Persisted in SQLite" });
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        using var createdJson = JsonDocument.Parse(await createResponse.Content.ReadAsStringAsync());
        var taskId = createdJson.RootElement.GetProperty("id").GetGuid();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", second.Token);
        var forbiddenByOwnership = await client.GetAsync($"/api/tasks/{taskId}");
        Assert.Equal(HttpStatusCode.NotFound, forbiddenByOwnership.StatusCode);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", first.Token);
        var listResponse = await client.GetAsync("/api/tasks");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
        using var listJson = JsonDocument.Parse(await listResponse.Content.ReadAsStringAsync());
        Assert.Equal("Integration task", listJson.RootElement[0].GetProperty("title").GetString());

        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new { email = first.Email, password = "Strong-password-123!" });
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
    }

    [Fact]
    public async Task Registration_RejectsWeakPasswordsAndDuplicateEmails()
    {
        factory.InitializeDatabase();
        using var client = factory.CreateClient();
        var email = $"duplicate-{Guid.NewGuid():N}@example.com";

        var weakPassword = await client.PostAsJsonAsync("/api/auth/register", new { email, password = "short" });
        Assert.Equal(HttpStatusCode.BadRequest, weakPassword.StatusCode);

        var first = await Register(client, email);
        Assert.False(string.IsNullOrWhiteSpace(first.Token));

        var duplicate = await client.PostAsJsonAsync("/api/auth/register", new { email, password = "Strong-password-123!" });
        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);
    }

    private static async Task<(string Token, string Email)> Register(HttpClient client, string email)
    {
        var response = await client.PostAsJsonAsync("/api/auth/register", new { email, password = "Strong-password-123!" });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return (json.RootElement.GetProperty("accessToken").GetString()!, json.RootElement.GetProperty("email").GetString()!);
    }
}
