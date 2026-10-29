using Cicadar.Api.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cicadar.Api.Persistence.Configurations;

public class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("account");

        builder.Property(entity => entity.Id).AsStringId();
        builder.Property(entity => entity.AccountId).AsString();
        builder.Property(entity => entity.ProviderId).AsString();
        builder.Property(entity => entity.UserId).AsString();
        builder.Property(entity => entity.AccessToken).AsText();
        builder.Property(entity => entity.RefreshToken).AsText();
        builder.Property(entity => entity.IdToken).AsText();
        builder.Property(entity => entity.AccessTokenExpiresAt).AsDateTime();
        builder.Property(entity => entity.RefreshTokenExpiresAt).AsDateTime();
        builder.Property(entity => entity.Scope).AsString();
        builder.Property(entity => entity.Password).AsText();
        builder.Property(entity => entity.CreatedAt).HasCreatedAtDefault();
        builder.Property(entity => entity.UpdatedAt).AsDateTime();

        builder.HasIndex(entity => entity.UserId).HasDatabaseName("account_userId_idx");
        builder.HasIndex(entity => new { entity.ProviderId, entity.AccountId })
            .IsUnique()
            .HasDatabaseName("account_providerId_accountId_key");

        builder.HasOne(entity => entity.User)
            .WithMany(user => user.Accounts)
            .HasForeignKey(entity => entity.UserId)
            .HasConstraintName("account_userId_fkey")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
