namespace Cicadar.Api.Persistence.Entities;

public class Comment
{
    public string Id { get; set; } = null!;

    public string UserId { get; set; } = null!;

    public string GuideSlug { get; set; } = null!;

    public string Body { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public User User { get; set; } = null!;
}
