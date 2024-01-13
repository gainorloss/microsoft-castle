// See https://aka.ms/new-console-template for more information
using Castle.DynamicProxy;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MicrosoftCastle.Samples.Common;
using System.Reflection;

namespace MicrosoftCastle.ConsoleApp
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var services = new ServiceCollection();
            services.AddLogging();//此处添加日志服务 伪代码 以便获取ILogger<SampleService>
            services.TryAddTransient<SampleService>();
            services.TryAddTransient<ISampleService, SampleService>();
            services.TryAddTransient<LoggingInterceptor>();//有依赖容器服务的拦截器，需要放到容器中
            services.ConfigureCastleDynamicProxy();//一定要在最后，不然会有些服务无法代理到 2024-1-13 13:53:05
            var sp = services.BuildServiceProvider();

            var proxy = sp.GetRequiredService<SampleService>();
            var name = await proxy.ShowAsync();

            Console.Read();
        }
    }

}
