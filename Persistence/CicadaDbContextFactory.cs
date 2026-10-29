using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Cicadar.Api.Persistence;

public class CicadaDbContextFactory : IDesignTimeDbContextFactory<CicadaDbContext>
{
    public CicadaDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString(CicadaDbContext.ConnectionStringName);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            connectionString = "server=127.0.0.1;port=3306;database=cicada;user=cicada;password=cicada";
        }

        var options = new DbContextOptionsBuilder<CicadaDbContext>()
            .UseMySql(connectionString, new MySqlServerVersion(CicadaDbContext.MySqlVersion))
            .Options;

        return new CicadaDbContext(options);
    }
}
