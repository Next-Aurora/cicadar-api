namespace Cicadar.Api.Persistence.Entities;

public class Session
{
    public string Id { get; set; } = null!;

    public DateTime ExpiresAt { get; set; }

    public string Token { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public string? IpAddress { get; set; }

    public string? UserAgent { get; set; }

    public string UserId { get; set; } = null!;

    public User User { get; set; } = null!;
}
