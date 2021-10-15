using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Scenario.Moq
{
    public class MockedResourceDescription<TScenarioBuilder, TMocked> :
        BaseResourceDescription<TScenarioBuilder, TMocked>
        where TScenarioBuilder : IScenarioBuilder
        where TMocked : class
    {
        private readonly Mock<TMocked> _mock;

        public MockedResourceDescription(
            TScenarioBuilder scenarioBuilder,
            Action<IReadOnlyCollection<TMocked>?>? resultCallback)
            : base(scenarioBuilder, resultCallback)
        {
            _mock = new Mock<TMocked>();
        }
        
        public MockedResourceDescription(
            TScenarioBuilder scenarioBuilder,
            Action<IReadOnlyCollection<TMocked>?>? resultCallback,
            params object[] args)
            : base(scenarioBuilder, resultCallback)
        {
            _mock = new Mock<TMocked>(args);
        }
        
        public MockedResourceDescription(
            TScenarioBuilder scenarioBuilder,
            Action<IReadOnlyCollection<TMocked>?>? resultCallback,
            MockBehavior mockBehavior,
            params object[] args)
            : base(scenarioBuilder, resultCallback)
        {
            _mock = new Mock<TMocked>(mockBehavior, args);
        }
        
        public MockedResourceDescription(
            TScenarioBuilder scenarioBuilder,
            Action<IReadOnlyCollection<TMocked>?>? resultCallback,
            MockBehavior mockBehavior)
            : base(scenarioBuilder, resultCallback)
        {
            _mock = new Mock<TMocked>(mockBehavior);
        }
        
        public MockedResourceDescription(
            TScenarioBuilder scenarioBuilder,
            Action<IReadOnlyCollection<TMocked>?>? resultCallback,
            Expression<Func<TMocked>> newExpression,
            MockBehavior behavior = MockBehavior.Default)
            : base(scenarioBuilder, resultCallback)
        {
            _mock = new Mock<TMocked>(newExpression, behavior);
        }

        protected override Task<IImmutableList<TMocked>> ScopeActionAsync(IServiceScope scope)
        {
            var results = new[] { _mock.Object };
            return Task.FromResult<IImmutableList<TMocked>>(results.ToImmutableList());
        }

        public MockedResourceDescription<TScenarioBuilder, TMocked> Mock(Action<Mock<TMocked>> mocker)
        {
            mocker(_mock);
            return this;
        }
    }
}