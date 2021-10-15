using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Scenario.Moq
{
    public class MockedSubResourceDescription<TScenarioBuilder, TDependent, TMocked> :
        BaseSubResourceDescription<TScenarioBuilder, TDependent, TMocked>
        where TScenarioBuilder : IScenarioBuilder
        where TMocked : class
    {
        private readonly Mock<TMocked> _mock;

        public MockedSubResourceDescription(
            IScenarioBuilderResourceDescription<TScenarioBuilder, TDependent> dependentResource,
            Action<IReadOnlyCollection<TMocked>?>? resultCallback)
            : base(dependentResource.Builder, dependentResource, resultCallback)
        {
            _mock = new Mock<TMocked>();
        }
        
        public MockedSubResourceDescription(
            IScenarioBuilderResourceDescription<TScenarioBuilder, TDependent> dependentResource,
            Action<IReadOnlyCollection<TMocked>?>? resultCallback,
            params object[] args)
            : base(dependentResource.Builder, dependentResource, resultCallback)
        {
            _mock = new Mock<TMocked>(args);
        }
        
        public MockedSubResourceDescription(
            IScenarioBuilderResourceDescription<TScenarioBuilder, TDependent> dependentResource,
            Action<IReadOnlyCollection<TMocked>?>? resultCallback,
            MockBehavior mockBehavior,
            params object[] args)
            : base(dependentResource.Builder, dependentResource, resultCallback)
        {
            _mock = new Mock<TMocked>(mockBehavior, args);
        }
        
        public MockedSubResourceDescription(
            IScenarioBuilderResourceDescription<TScenarioBuilder, TDependent> dependentResource,
            Action<IReadOnlyCollection<TMocked>?>? resultCallback,
            MockBehavior mockBehavior)
            : base(dependentResource.Builder, dependentResource, resultCallback)
        {
            _mock = new Mock<TMocked>(mockBehavior);
        }
        
        public MockedSubResourceDescription(
            IScenarioBuilderResourceDescription<TScenarioBuilder, TDependent> dependentResource,
            Action<IReadOnlyCollection<TMocked>?>? resultCallback,
            Expression<Func<TMocked>> newExpression,
            MockBehavior behavior = MockBehavior.Default)
            : base(dependentResource.Builder, dependentResource, resultCallback)
        {
            _mock = new Mock<TMocked>(newExpression, behavior);
        }

        protected override Task<IImmutableList<TMocked>> ScopeActionAsync(IServiceScope scope)
        {
            return Task.FromResult<IImmutableList<TMocked>>(new ImmutableArray<TMocked> { _mock.Object });
        }

        public MockedSubResourceDescription<TScenarioBuilder, TDependent, TMocked> Mock(Action<Mock<TMocked>> mocker)
        {
            mocker(_mock);
            return this;
        }
    }
}