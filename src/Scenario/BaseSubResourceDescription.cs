using System;
using System.Collections.Generic;

namespace Scenario
{
    public abstract class
        BaseSubResourceDescription<TScenarioBuilder, TParentResource, TSubResource> :
            BaseResourceDescription<TScenarioBuilder, TSubResource>,
            IScenarioBuilderSubResourceDescription<TScenarioBuilder, TParentResource, TSubResource>
        where TScenarioBuilder : IScenarioBuilder
    {
        public IScenarioBuilderResourceDescription<TScenarioBuilder, TParentResource> ParentDescription { get; }

        public IScenarioBuilderSubResourceDescription<TScenarioBuilder, TDependent, TParentResource>?
            ParentAs<TDependent>() =>
            ParentDescription as IScenarioBuilderSubResourceDescription<TScenarioBuilder, TDependent, TParentResource>; 

        protected BaseSubResourceDescription(
            TScenarioBuilder scenarioBuilder,
            IScenarioBuilderResourceDescription<TScenarioBuilder, TParentResource> parentDescription,
            Action<IReadOnlyCollection<TSubResource>?>? resultCallback) : base(scenarioBuilder, resultCallback)
        {
            ParentDescription = parentDescription;
        }
    }
}