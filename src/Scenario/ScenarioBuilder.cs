using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Scenario
{
    public class ScenarioBuilder : IScenarioBuilder
    {
        public IServiceCollection Services { get; }

        private readonly List<(Func<IServiceScope, Task<object?>> action, Action<object?>? callback)> _scopedActions =
            new();
        
        public ScenarioBuilder() => Services = new ServiceCollection();
        
        public ScenarioBuilder(IServiceCollection serviceCollection) => Services = serviceCollection;

        public IScenarioBuilder Use(Action<IServiceCollection> configure)
        {
            configure(Services);
            return this;
        }

        public IScenarioBuilder With(Func<IServiceScope, Task<object?>> scopedAction,
            Action<object?>? resultCallback = null)
        {
            _scopedActions.Add((scopedAction, resultCallback));
            return this;
        }

        public async Task<IScenario> BuildAsync()
        {
            var provider = Services.BuildServiceProvider();
            var resources = new ServiceCollection();

            using var scope = provider.CreateScope();
            foreach (var (action, callback) in _scopedActions)
            {
                var result = await action(scope);
                callback?.Invoke(result);

                if (result == null) continue;

                var type = result.GetType();
                if (type.IsAssignableTo(typeof(IEnumerable)))
                {
                    var enumerable = (IEnumerable) result;
                    foreach (var value in enumerable)
                    {
                        resources.Add(new ServiceDescriptor(type, _ => value, ServiceLifetime.Scoped));
                    }

                    continue;
                }

                resources.Add(new ServiceDescriptor(type, _ => result, ServiceLifetime.Scoped));
            }

            return new Scenario(provider, resources.BuildServiceProvider());
        }
    }
}
