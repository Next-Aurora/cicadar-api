using Cicadar.Api.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cicadar.Api.Persistence.Configurations;

public class SessionConfiguration : IEntityTypeConfiguration<Session>
{
    public void Configure(EntityTypeBuilder<Session> builder)
    {
        builder.ToTable("session");

        builder.Property(entity => entity.Id).AsStringId();
        builder.Property(entity => entity.ExpiresAt).AsDateTime();
        builder.Property(entity => entity.Token).AsString();
        builder.Property(entity => entity.CreatedAt).HasCreatedAtDefault();
        builder.Property(entity => entity.UpdatedAt).AsDateTime();
        builder.Property(entity => entity.IpAddress).AsString();
        builder.Property(entity => entity.UserAgent).AsString();
        builder.Property(entity => entity.UserId).AsString();

        builder.HasIndex(entity => entity.Token).IsUnique().HasDatabaseName("session_token_key");
        builder.HasIndex(entity => entity.UserId).HasDatabaseName("session_userId_idx");

        builder.HasOne(entity => entity.User)
            .WithMany(user => user.Sessions)
            .HasForeignKey(entity => entity.UserId)
            .HasConstraintName("session_userId_fkey")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
