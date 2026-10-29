using Cicadar.Api.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cicadar.Api.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("user");

        builder.Property(entity => entity.Id).AsStringId();
        builder.Property(entity => entity.Name).AsString();
        builder.Property(entity => entity.Email).AsString();
        builder.Property(entity => entity.EmailVerified).HasColumnType(ColumnTypes.Boolean).HasDefaultValue(false);
        builder.Property(entity => entity.Image).AsString();
        builder.Property(entity => entity.CreatedAt).HasCreatedAtDefault();
        builder.Property(entity => entity.UpdatedAt).AsDateTime();

        builder.HasIndex(entity => entity.Email).IsUnique().HasDatabaseName("user_email_key");
    }
}
