# microsoft-castle

### QuickStart

0. `install-package CastleCore.Extensions.DependencyInjection`

1. IHostBuilder 
```csharp
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
```
2. define your interceptors 
 * _Type Interceptor_
```csharp
public class LoggingInterceptor : InterceptorBase
    {
        private readonly ILogger<LoggingInterceptor> _logger;

        public LoggingInterceptor(ILogger<LoggingInterceptor> logger)
        {
            _logger = logger;
        }
        protected override async ValueTask InterceptAsync(IAsyncInvocation invocation)
        {
            await Console.Out.WriteLineAsync(nameof(LoggingInterceptor));
            await invocation.ProceedAsync();
        }
    }
```
* _Annotation interceptor_

```csharp
/// <summary>
    /// 异常捕获、日志记录和耗时监控 拦截器 2024-1-12 21:28:22
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class CatchLoggingInterceptor : InterceptorBaseAttribute
    {
        protected override async ValueTask InterceptAsync(IAsyncInvocation invocation)
        {
            //TODO:类注释所写的逻辑
            await Console.Out.WriteLineAsync("Interceptor  starting...");
            await invocation.ProceedAsync();
            await Console.Out.WriteLineAsync("Interceptor  ended...");
        }
    }
```
3. Add it to the method you want to intercept.
---
If u use `SampleService`
```csharp 
public class SampleService : ISampleService
    {
        [CatchLoggingInterceptor]
        [Interceptor(typeof(LoggingInterceptor))]
        public virtual Task<string> ShowAsync()
        {
            Console.WriteLine(nameof(ShowAsync));
            return Task.FromResult(nameof(ShowAsync));
        }
    }

    
```
---
 If u use `ISampleService`
```csharp
    public interface ISampleService
    {
        [CatchLoggingInterceptor]
        Task<string> ShowAsync();
    }
```