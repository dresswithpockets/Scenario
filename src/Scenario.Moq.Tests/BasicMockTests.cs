using System;
using System.Threading.Tasks;
using Xunit;

namespace Scenario.Moq.Tests
{
    public class BasicMockTests
    {
        [Fact]
        public async Task MockResourcesCanBeMocked()
        {
            IDisposable disposableThing = null!;
            var scenario = await new ScenarioBuilder()
                .WithMock<ScenarioBuilder, IDisposable>(d => disposableThing = d)
                .Mock(mock => mock.Setup(d => d.Dispose()).Throws<InvalidOperationException>())
                .BuildAsync();

            Assert.Throws<InvalidOperationException>(() => disposableThing.Dispose());
        }
    }
}