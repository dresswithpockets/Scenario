namespace Scenario
{
    public interface
        IScenarioBuilderSubResourceDescription<out TScenarioBuilder, out TParentResource, out TSubResource> :
            IScenarioBuilderResourceDescription<TScenarioBuilder, TSubResource>
        where TScenarioBuilder : IScenarioBuilder
    {
        IScenarioBuilderResourceDescription<TScenarioBuilder, TParentResource> ParentDescription { get; }

        public IScenarioBuilderSubResourceDescription<TScenarioBuilder, TDependent, TParentResource>?
            ParentAs<TDependent>() =>
            ParentDescription as IScenarioBuilderSubResourceDescription<TScenarioBuilder, TDependent, TParentResource>;
    }
}