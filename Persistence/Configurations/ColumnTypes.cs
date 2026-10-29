using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cicadar.Api.Persistence.Configurations;

internal static class ColumnTypes
{
    public const string String = "varchar(191)";
    public const string Text = "text";
    public const string DateTime = "datetime(3)";
    public const string Boolean = "tinyint(1)";
    public const string CurrentTimestamp = "CURRENT_TIMESTAMP(3)";

    public static PropertyBuilder<string> AsStringId(this PropertyBuilder<string> property) =>
        property.HasColumnType(String).HasMaxLength(191).ValueGeneratedNever();

    public static PropertyBuilder<TProperty> AsString<TProperty>(this PropertyBuilder<TProperty> property) =>
        property.HasColumnType(String).HasMaxLength(191);

    public static PropertyBuilder<TProperty> AsText<TProperty>(this PropertyBuilder<TProperty> property) =>
        property.HasColumnType(Text);

    public static PropertyBuilder<DateTime> AsDateTime(this PropertyBuilder<DateTime> property) =>
        property.HasColumnType(DateTime);

    public static PropertyBuilder<DateTime?> AsDateTime(this PropertyBuilder<DateTime?> property) =>
        property.HasColumnType(DateTime);

    public static PropertyBuilder<DateTime> HasCreatedAtDefault(this PropertyBuilder<DateTime> property) =>
        property.AsDateTime().HasDefaultValueSql(CurrentTimestamp);
}
