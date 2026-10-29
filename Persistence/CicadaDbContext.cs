using Cicadar.Api.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

namespace Cicadar.Api.Persistence;

public class CicadaDbContext : DbContext
{
    public const string ConnectionStringName = "Cicada";

    public static readonly Version MySqlVersion = new(8, 4, 0);

    public CicadaDbContext(DbContextOptions<CicadaDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    public DbSet<Session> Sessions => Set<Session>();

    public DbSet<Account> Accounts => Set<Account>();

    public DbSet<Verification> Verifications => Set<Verification>();

    public DbSet<WechatLoginTicket> WechatLoginTickets => Set<WechatLoginTicket>();

    public DbSet<Favorite> Favorites => Set<Favorite>();

    public DbSet<Comment> Comments => Set<Comment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasCharSet(CharSet.Utf8Mb4, DelegationModes.ApplyToAll);
        modelBuilder.UseCollation("utf8mb4_unicode_ci");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CicadaDbContext).Assembly);

        // Prisma / Better Auth 使用 camelCase 列名。MySQL 列名大小写不敏感，但结果集字段名按建表时的写法返回。
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                var name = property.Name;
                if (name.Length == 0 || !char.IsUpper(name[0]))
                {
                    continue;
                }

                property.SetColumnName(char.ToLowerInvariant(name[0]) + name[1..]);
            }
        }
    }
}
