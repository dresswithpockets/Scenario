using System;
using System.Linq;
using System.Linq.Expressions;
using Moq;

namespace Scenario.Moq
{
    public static class ScenarioMoqExtensions
    {
        public static MockedResourceDescription<TScenarioBuilder, TMocked> WithMock<TScenarioBuilder, TMocked>(
            this TScenarioBuilder scenarioBuilder,
            Action<TMocked>? resultCallback = null)
            where TScenarioBuilder : IScenarioBuilder
            where TMocked : class
            => new(scenarioBuilder, results => resultCallback?.Invoke(results!.Single()));
        
        public static MockedResourceDescription<TScenarioBuilder, TMocked> WithMock<TScenarioBuilder, TMocked>(
            this TScenarioBuilder scenarioBuilder,
            Action<TMocked>? resultCallback = null,
            params object[] args)
            where TScenarioBuilder : IScenarioBuilder
            where TMocked : class
            => new(scenarioBuilder, results => resultCallback?.Invoke(results!.Single()), args);

        public static MockedResourceDescription<TScenarioBuilder, TMocked> WithMock<TScenarioBuilder, TMocked>(
            this TScenarioBuilder scenarioBuilder,
            MockBehavior behavior,
            Action<TMocked>? resultCallback = null,
            params object[] args)
            where TScenarioBuilder : IScenarioBuilder
            where TMocked : class
            => new(scenarioBuilder, results => resultCallback?.Invoke(results!.Single()), behavior, args);
        
        public static MockedResourceDescription<TScenarioBuilder, TMocked> WithMock<TScenarioBuilder, TMocked>(
            this TScenarioBuilder scenarioBuilder,
            MockBehavior behavior,
            Action<TMocked>? resultCallback = null)
            where TScenarioBuilder : IScenarioBuilder
            where TMocked : class
            => new(scenarioBuilder, results => resultCallback?.Invoke(results!.Single()), behavior);

        public static MockedResourceDescription<TScenarioBuilder, TMocked> WithMock<TScenarioBuilder, TMocked>(
            this TScenarioBuilder scenarioBuilder,
            Expression<Func<TMocked>> newExpression,
            MockBehavior behavior = MockBehavior.Default,
            Action<TMocked>? resultCallback = null)
            where TScenarioBuilder : IScenarioBuilder
            where TMocked : class
            => new(scenarioBuilder, results => resultCallback?.Invoke(results!.Single()), newExpression, behavior);
        
        public static MockedSubResourceDescription<TScenarioBuilder, TDependent, TMocked> WithDependentMock<
            TScenarioBuilder, TDependent, TMocked>(
            this IScenarioBuilderResourceDescription<TScenarioBuilder, TDependent> dependentResource,
            Action<TMocked>? resultCallback = null)
            where TScenarioBuilder : IScenarioBuilder
            where TMocked : class
            => new(dependentResource, results => resultCallback?.Invoke(results!.Single()));

        public static MockedSubResourceDescription<TScenarioBuilder, TDependent, TMocked> WithDependentMock<
            TScenarioBuilder, TDependent, TMocked>(
            this IScenarioBuilderResourceDescription<TScenarioBuilder, TDependent> dependentResource,
            Action<TMocked>? resultCallback = null,
            params object[] args)
            where TScenarioBuilder : IScenarioBuilder
            where TMocked : class
            => new(dependentResource, results => resultCallback?.Invoke(results!.Single()), args);

        public static MockedSubResourceDescription<TScenarioBuilder, TDependent, TMocked> WithDependentMock<
            TScenarioBuilder, TDependent, TMocked>(
            this IScenarioBuilderResourceDescription<TScenarioBuilder, TDependent> dependentResource,
            MockBehavior behavior,
            Action<TMocked>? resultCallback = null,
            params object[] args)
            where TScenarioBuilder : IScenarioBuilder
            where TMocked : class
            => new(dependentResource, results => resultCallback?.Invoke(results!.Single()), behavior, args);
        
        public static MockedSubResourceDescription<TScenarioBuilder, TDependent, TMocked> WithDependentMock<
            TScenarioBuilder, TDependent, TMocked>(
            this IScenarioBuilderResourceDescription<TScenarioBuilder, TDependent> dependentResource,
            MockBehavior behavior,
            Action<TMocked>? resultCallback = null)
            where TScenarioBuilder : IScenarioBuilder
            where TMocked : class
            => new(dependentResource, results => resultCallback?.Invoke(results!.Single()), behavior);

        public static MockedSubResourceDescription<TScenarioBuilder, TDependent, TMocked> WithDependentMock<
            TScenarioBuilder, TDependent, TMocked>(
            this IScenarioBuilderResourceDescription<TScenarioBuilder, TDependent> dependentResource,
            Expression<Func<TMocked>> newExpression,
            MockBehavior behavior = MockBehavior.Default,
            Action<TMocked>? resultCallback = null)
            where TScenarioBuilder : IScenarioBuilder
            where TMocked : class
            => new(dependentResource, results => resultCallback?.Invoke(results!.Single()), newExpression, behavior);
    }
}