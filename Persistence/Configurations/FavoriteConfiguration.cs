using Cicadar.Api.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cicadar.Api.Persistence.Configurations;

public class FavoriteConfiguration : IEntityTypeConfiguration<Favorite>
{
    public void Configure(EntityTypeBuilder<Favorite> builder)
    {
        builder.ToTable("favorite");

        builder.Property(entity => entity.Id).AsStringId();
        builder.Property(entity => entity.UserId).AsString();
        builder.Property(entity => entity.GuideSlug).AsString();
        builder.Property(entity => entity.CreatedAt).HasCreatedAtDefault();

        builder.HasIndex(entity => entity.GuideSlug).HasDatabaseName("favorite_guideSlug_idx");
        builder.HasIndex(entity => new { entity.UserId, entity.GuideSlug })
            .IsUnique()
            .HasDatabaseName("favorite_userId_guideSlug_key");

        builder.HasOne(entity => entity.User)
            .WithMany(user => user.Favorites)
            .HasForeignKey(entity => entity.UserId)
            .HasConstraintName("favorite_userId_fkey")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
