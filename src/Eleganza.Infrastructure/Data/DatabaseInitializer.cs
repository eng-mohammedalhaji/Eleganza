using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Eleganza.Infrastructure.Data;

public static class DatabaseInitializer
{
    public static async Task InitializeAsync(
        IServiceProvider services,
        IConfiguration configuration,
        IHostEnvironment environment,
        CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var applyMigrations = configuration.GetValue(
            "Database:ApplyMigrationsOnStartup",
            environment.IsDevelopment());

        if (applyMigrations)
        {
            await database.Database.MigrateAsync(cancellationToken);
        }

        await IdentitySeeder.SeedRolesAsync(scope.ServiceProvider, cancellationToken);

        if (configuration.GetValue("Database:SeedDevelopmentCatalog", environment.IsDevelopment()))
        {
            await DevelopmentDataSeeder.SeedCatalogAsync(database, cancellationToken);
        }
    }
}
