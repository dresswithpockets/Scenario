using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Scenario
{
    public interface IScenarioBuilder
    {
        IScenarioBuilder Use(Action<IServiceCollection> configure);
        IScenarioBuilder With(Func<IServiceScope, Task<object?>> scopedAction, Action<object?>? resultCallback = null);
        Task<IScenario> BuildAsync();
    }
}