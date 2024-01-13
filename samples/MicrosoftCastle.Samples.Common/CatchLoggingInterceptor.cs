using Castle.DynamicProxy;

namespace MicrosoftCastle.Samples.Common
{

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

}
