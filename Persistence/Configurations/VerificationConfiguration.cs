using Cicadar.Api.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cicadar.Api.Persistence.Configurations;

public class VerificationConfiguration : IEntityTypeConfiguration<Verification>
{
    public void Configure(EntityTypeBuilder<Verification> builder)
    {
        builder.ToTable("verification");

        builder.Property(entity => entity.Id).AsStringId();
        builder.Property(entity => entity.Identifier).AsString();
        builder.Property(entity => entity.Value).AsString();
        builder.Property(entity => entity.ExpiresAt).AsDateTime();
        builder.Property(entity => entity.CreatedAt).HasCreatedAtDefault();
        builder.Property(entity => entity.UpdatedAt).AsDateTime();

        builder.HasIndex(entity => entity.Identifier).HasDatabaseName("verification_identifier_idx");
    }
}
