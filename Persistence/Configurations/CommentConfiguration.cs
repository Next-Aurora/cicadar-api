using Cicadar.Api.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cicadar.Api.Persistence.Configurations;

public class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        builder.ToTable("comment");

        builder.Property(entity => entity.Id).AsStringId();
        builder.Property(entity => entity.UserId).AsString();
        builder.Property(entity => entity.GuideSlug).AsString();
        builder.Property(entity => entity.Body).AsText();
        builder.Property(entity => entity.CreatedAt).HasCreatedAtDefault();
        builder.Property(entity => entity.UpdatedAt).AsDateTime();

        builder.HasIndex(entity => entity.GuideSlug).HasDatabaseName("comment_guideSlug_idx");
        builder.HasIndex(entity => entity.UserId).HasDatabaseName("comment_userId_idx");

        builder.HasOne(entity => entity.User)
            .WithMany(user => user.Comments)
            .HasForeignKey(entity => entity.UserId)
            .HasConstraintName("comment_userId_fkey")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
