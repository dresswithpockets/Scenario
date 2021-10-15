using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Fs3 = FakeS3;

namespace Scenario.FakeS3
{
    public static class ScenarioFakeS3Extensions
    {
        public static TScenarioBuilder UseOnDiskS3<TScenarioBuilder>(
            this TScenarioBuilder scenarioBuilder,
            string? root = null,
            bool quietMode = false)
            where TScenarioBuilder : IScenarioBuilder
            => (TScenarioBuilder)scenarioBuilder.Use(
                services => services.AddSingleton(
                    _ => Fs3.AWS.FakeS3.CreateFileClient(
                        root ?? Path.Join(Path.GetTempPath(), Path.GetRandomFileName()), quietMode)));

        public static TScenarioBuilder UseInMemoryS3<TScenarioBuilder>(
            this TScenarioBuilder scenarioBuilder)
            where TScenarioBuilder : IScenarioBuilder
            => (TScenarioBuilder)scenarioBuilder.Use(
                services => services.AddSingleton(_ => Fs3.AWS.FakeS3.CreateMemoryClient()));

        public static TScenarioBuilder UseFileStoreBuckets<TScenarioBuilder>(
            this TScenarioBuilder scenarioBuilder,
            string root,
            bool quietMode = false)
            where TScenarioBuilder : IScenarioBuilder
            => (TScenarioBuilder)scenarioBuilder.Use(
                services => services.AddSingleton<Fs3.IBucketStore>(_ => new Fs3.FileStore(root, quietMode)));
        
        public static TScenarioBuilder UseMemoryStoreBuckets<TScenarioBuilder>(
            this TScenarioBuilder scenarioBuilder)
            where TScenarioBuilder : IScenarioBuilder
            => (TScenarioBuilder)scenarioBuilder.Use(
                services => services.AddSingleton<Fs3.IBucketStore>(_ => new Fs3.MemoryStore()));
    }
}