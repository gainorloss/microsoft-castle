using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MicrosoftCastle.ConsoleApp;
using MicrosoftCastle.Samples.Common;

namespace MicrosoftCastle.Benchmark
{
    [Config(typeof(Config)), MemoryDiagnoser]
    public class ServiceResolveBenchmark
    {
        internal class Config : ManualConfig
        {

        }
        private IServiceProvider _normal;
        private IServiceProvider _proxy;
        public ServiceResolveBenchmark()
        {
            var services = new ServiceCollection();
            services.TryAddTransient<SampleService>();
            services.TryAddTransient<LoggingInterceptor>();//有依赖容器服务的拦截器，需要放到容器中
            _normal = services.BuildServiceProvider();
            _proxy = services.ConfigureCastleDynamicProxy().BuildServiceProvider();
        }

        [Benchmark(Baseline = true)]
        public SampleService Normal() => _normal.GetRequiredService<SampleService>();

        [Benchmark]
        public SampleService DynamicProxy() => _proxy.GetRequiredService<SampleService>();
    }
}
