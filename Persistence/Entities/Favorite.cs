namespace Cicadar.Api.Persistence.Entities;

public class Favorite
{
    public string Id { get; set; } = null!;

    public string UserId { get; set; } = null!;

    public string GuideSlug { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public User User { get; set; } = null!;
}
