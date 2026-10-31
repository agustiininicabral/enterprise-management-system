namespace EnterpriseManagement.Api.Domain;
public sealed class User
{
    public const string DefaultRole = "User";
    public const string AdminRole = "Admin";

    public Guid Id { get; set; } = Guid.NewGuid();
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = DefaultRole;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<WorkTask> Tasks { get; set; } = new List<WorkTask>();
}
