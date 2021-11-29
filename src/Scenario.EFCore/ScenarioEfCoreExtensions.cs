using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Scenario.EFCore
{
    public static class ScenarioEfCoreExtensions
    {
        [ScenarioDependency(CreateNonGenericExtension = true)]
        public static void UseOnDiskSqliteDbContext<TContext>(IServiceCollection services) where TContext : DbContext
        {
            var provider = new SqliteConnectionProvider<TContext>();
            services.AddSingleton<IConnectionProvider<TContext>>(_ => provider);
            services.AddSingleton(provider.Options);
            services.AddScoped<TContext>();
        }

        [ScenarioResource(CreateNonGenericExtension = true)]
        public static async Task WithMigrations<TDbContext>(IServiceScope scope) where TDbContext : DbContext
        {
            await scope.ServiceProvider.GetRequiredService<TDbContext>().Database.MigrateAsync();
        }
    }
}