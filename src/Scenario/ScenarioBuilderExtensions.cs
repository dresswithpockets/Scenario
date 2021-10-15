using System.Threading.Tasks;

namespace Scenario
{
    public static class ScenarioBuilderExtensions
    {
        public static Task<IScenario> BuildAsync<TScenarioBuilder, TResource>(
            this IScenarioBuilderResourceDescription<TScenarioBuilder, TResource> resourceDescription)
            where TScenarioBuilder : IScenarioBuilder
            => resourceDescription.Builder.BuildAsync();
    }
}