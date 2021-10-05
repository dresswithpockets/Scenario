using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Scenario
{
    public abstract class
        BaseResourceDescription<TScenarioBuilder, TResource> : IScenarioBuilderResourceDescription<TScenarioBuilder,
            TResource>
        where TScenarioBuilder : IScenarioBuilder
    {
        private readonly Action<IReadOnlyCollection<TResource>?>? _resultCallback;
        public TScenarioBuilder Builder { get; }
        

        public IReadOnlyCollection<TResource> Resources { get; protected set; }

        protected virtual void ResultCallback(object? result)
        {
            var enumerable = result as IReadOnlyCollection<TResource>;
            _resultCallback?.Invoke(enumerable);
            Resources = enumerable ?? Array.Empty<TResource>();
        }

        protected abstract Task<IImmutableList<TResource>> ScopeActionAsync(IServiceScope scope); 

        public BaseResourceDescription(TScenarioBuilder scenarioBuilder,
            Action<IReadOnlyCollection<TResource>?>? resultCallback)
        {
            Builder = scenarioBuilder;
            _resultCallback = resultCallback;
            scenarioBuilder.With(async scope => await ScopeActionAsync(scope), ResultCallback);
        }
    }
}