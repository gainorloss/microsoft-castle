using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MicrosoftCastle.ConsoleApp;
using MicrosoftCastle.Samples.Common;

namespace MicrosoftCastle.Benchmark
{
    [Config(typeof(Config)), MemoryDiagnoser]
    public class ServiceProviderBuildBenchmark
    {
        internal class Config : ManualConfig
        {

        }
        private IServiceCollection _services;
        public ServiceProviderBuildBenchmark()
        {
            _services = new ServiceCollection();
            _services.TryAddTransient<SampleService>();
            _services.TryAddTransient<LoggingInterceptor>();//有依赖容器服务的拦截器，需要放到容器中
        }

        [Benchmark(Baseline = true)]
        public IServiceProvider Normal() => _services.BuildServiceProvider();

        [Benchmark]
        public IServiceProvider DynamicProxy() => _services.ConfigureCastleDynamicProxy().BuildServiceProvider();
    }
}
