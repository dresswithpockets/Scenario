using System.Collections.Generic;

namespace Scenario
{
    public interface IScenarioBuilderResourceDescription<out TScenarioBuilder, out TResource>
        where TScenarioBuilder : IScenarioBuilder
    {
        TScenarioBuilder Builder { get; }

        IReadOnlyCollection<TResource> Resources { get; }
    }
}