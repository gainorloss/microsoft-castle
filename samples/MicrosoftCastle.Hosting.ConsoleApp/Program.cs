using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using MicrosoftCastle.ConsoleApp;
using MicrosoftCastle.Samples.Common;

namespace MicrosoftCastle.Hosting.ConsoleApp
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            await Host.CreateDefaultBuilder()
                    .UseCastleDynamicProxyServiceProvider()//castle dymamic proxy.
                    .ConfigureServices((ctx, services) =>
                    {
                        services.AddLogging();//此处添加日志服务 伪代码 以便获取ILogger<SampleService>
                        services.TryAddTransient<SampleService>();
                        services.TryAddTransient<ISampleService, SampleService>();
                        services.TryAddTransient<LoggingInterceptor>();//有依赖容器服务的拦截器，需要放到容器中
                        services.AddHostedService<Bootstrapper>();
                    })
                    .Build().RunAsync();
        }
    }

    internal class Bootstrapper : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public Bootstrapper(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var sp = scope.ServiceProvider;
                var proxy = sp.GetRequiredService<SampleService>();
                var name = await proxy.ShowAsync();
            }
        }
    }
}
