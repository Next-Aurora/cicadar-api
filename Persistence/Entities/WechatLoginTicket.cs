namespace Cicadar.Api.Persistence.Entities;

public class WechatLoginTicket
{
    public string Id { get; set; } = null!;

    public string NonceHash { get; set; } = null!;

    public string Status { get; set; } = null!;

    public string? UserId { get; set; }

    public string? BindUserId { get; set; }

    public DateTime ExpiresAt { get; set; }

    public DateTime CreatedAt { get; set; }
}
