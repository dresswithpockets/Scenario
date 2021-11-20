using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Fs3 = FakeS3;

namespace Scenario.FakeS3
{
    public static class ScenarioFakeS3Extensions
    {
        [ScenarioDependency]
        public static void UseOnDiskS3(IServiceCollection services, string? root = null, bool quietMode = false)
            => services.AddSingleton(_ => Fs3.AWS.FakeS3.CreateFileClient(
                root ?? Path.Join(Path.GetTempPath(), Path.GetRandomFileName()), quietMode));

        [ScenarioDependency]
        public static void UseInMemoryS3(IServiceCollection services)
            => services.AddSingleton(_ => Fs3.AWS.FakeS3.CreateMemoryClient());

        [ScenarioDependency]
        public static void UseFileStoreBuckets(IServiceCollection services, string root, bool quietMode = false)
            => services.AddSingleton<Fs3.IBucketStore>(_ => new Fs3.FileStore(root, quietMode));

        [ScenarioDependency]
        public static void UseFileStoreBuckets(IServiceCollection services)
            => services.AddSingleton<Fs3.IBucketStore>(_ => new Fs3.MemoryStore());
    }
}