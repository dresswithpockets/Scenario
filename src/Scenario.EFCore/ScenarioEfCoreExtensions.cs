using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Scenario.EFCore
{
    public static class ScenarioEfCoreExtensions
    {
        public static TScenarioBuilder UseOnDiskSqliteDbContext<TScenarioBuilder, TContext>(
            this TScenarioBuilder scenarioBuilder)
            where TContext : DbContext
            where TScenarioBuilder : IScenarioBuilder
            => (TScenarioBuilder)scenarioBuilder.Use(services =>
            {
                var provider = new SqliteConnectionProvider<TContext>();
                services.AddSingleton<IConnectionProvider<TContext>>(_ => provider);
                services.AddSingleton(provider.Options);
                services.AddScoped<TContext>();
            });
        
        public static IScenarioBuilder UseOnDiskSqliteDbContext<TContext>(
            this IScenarioBuilder scenarioBuilder)
            where TContext : DbContext
            => scenarioBuilder.Use(services =>
            {
                var provider = new SqliteConnectionProvider<TContext>();
                services.AddSingleton<IConnectionProvider<TContext>>(_ => provider);
                services.AddSingleton(provider.Options);
                services.AddScoped<TContext>();
            });
        
        public static TScenarioBuilder WithMigrations<TScenarioBuilder, TDbContext>(
            this TScenarioBuilder scenarioBuilder)
            where TScenarioBuilder : IScenarioBuilder
            where TDbContext : DbContext
            => (TScenarioBuilder) scenarioBuilder.With(async scope =>
            {
                await scope.ServiceProvider.GetRequiredService<TDbContext>().Database.MigrateAsync();
                return null;
            });

        public static IScenarioBuilder WithMigrations<TDbContext>(
            this IScenarioBuilder scenarioBuilder)
            where TDbContext : DbContext
            => scenarioBuilder.With(async scope =>
            {
                await scope.ServiceProvider.GetRequiredService<TDbContext>().Database.MigrateAsync();
                return null;
            });
    }
}