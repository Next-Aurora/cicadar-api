namespace Cicadar.Api.Persistence.Entities;

public class Account
{
    public string Id { get; set; } = null!;

    public string AccountId { get; set; } = null!;

    public string ProviderId { get; set; } = null!;

    public string UserId { get; set; } = null!;

    public User User { get; set; } = null!;

    public string? AccessToken { get; set; }

    public string? RefreshToken { get; set; }

    public string? IdToken { get; set; }

    public DateTime? AccessTokenExpiresAt { get; set; }

    public DateTime? RefreshTokenExpiresAt { get; set; }

    public string? Scope { get; set; }

    public string? Password { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
