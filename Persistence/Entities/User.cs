namespace Cicadar.Api.Persistence.Entities;

public class User
{
    public string Id { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    public bool EmailVerified { get; set; }

    public string? Image { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public ICollection<Session> Sessions { get; set; } = new List<Session>();

    public ICollection<Account> Accounts { get; set; } = new List<Account>();

    public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();

    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
}
