using System;
using Microsoft.Extensions.DependencyInjection;

namespace Scenario
{
    public class Scenario : IScenario
    {
        public IServiceProvider ServiceProvider { get; }

        public IServiceProvider ResourceProvider { get; }

        public IServiceScope CreateScope() => ServiceProvider.CreateScope();

        internal Scenario(IServiceProvider serviceProvider, IServiceProvider resourceProvider)
        {
            ServiceProvider = serviceProvider;
            ResourceProvider = resourceProvider;
        }
    }
}