namespace Cicadar.Api.Persistence.Entities;

public class Verification
{
    public string Id { get; set; } = null!;

    public string Identifier { get; set; } = null!;

    public string Value { get; set; } = null!;

    public DateTime ExpiresAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
