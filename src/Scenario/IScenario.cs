using System;
using Microsoft.Extensions.DependencyInjection;

namespace Scenario
{
    public interface IScenario : IServiceScopeFactory
    {
        IServiceProvider ServiceProvider { get; }

        IServiceProvider ResourceProvider { get; }
    }
}