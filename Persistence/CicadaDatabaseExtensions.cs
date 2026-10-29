using Microsoft.EntityFrameworkCore;

namespace Cicadar.Api.Persistence;

public static class CicadaDatabaseExtensions
{
    public static IServiceCollection AddCicadaDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(CicadaDbContext.ConnectionStringName);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return services;
        }

        services.AddDbContext<CicadaDbContext>(options =>
            options.UseMySql(connectionString, new MySqlServerVersion(CicadaDbContext.MySqlVersion)));

        return services;
    }
}
