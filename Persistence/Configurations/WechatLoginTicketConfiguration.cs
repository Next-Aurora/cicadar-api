using Cicadar.Api.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cicadar.Api.Persistence.Configurations;

public class WechatLoginTicketConfiguration : IEntityTypeConfiguration<WechatLoginTicket>
{
    public void Configure(EntityTypeBuilder<WechatLoginTicket> builder)
    {
        builder.ToTable("wechat_login_ticket");

        builder.Property(entity => entity.Id).AsStringId();
        builder.Property(entity => entity.NonceHash).AsString();
        builder.Property(entity => entity.Status).AsString();
        builder.Property(entity => entity.UserId).AsString();
        builder.Property(entity => entity.BindUserId).AsString();
        builder.Property(entity => entity.ExpiresAt).AsDateTime();
        builder.Property(entity => entity.CreatedAt).HasCreatedAtDefault();

        builder.HasIndex(entity => entity.ExpiresAt).HasDatabaseName("wechat_login_ticket_expiresAt_idx");
    }
}
