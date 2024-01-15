using EventHandlerInterception.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DoraInterceptionAOP.ConsoleApp
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            await Host.CreateDefaultBuilder(args)
                 .ConfigureServices(services =>
                 {
                     services.AddLogging();
                     services.AddTransient<CatchLoggingOccurredEventHandler>();
                     services.AddHostedService<Bootstrapper>();
                 })
                 .UseInterception()//dora interception 2024-1-15 10:59:30 
                 .Build()
                 .RunAsync();
        }
    }

    internal class Bootstrapper : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScope;

        public Bootstrapper(IServiceScopeFactory serviceScope)
        {
            _serviceScope = serviceScope;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using (var scope = _serviceScope.CreateScope())
            {
                var sp = scope.ServiceProvider;
                var handler = sp.GetRequiredService<CatchLoggingOccurredEventHandler>();
                await handler.HandleAsync(new CatchLoggingOccurredEvent(1));
            }
        }
    }
}
