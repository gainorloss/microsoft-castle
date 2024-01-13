using Castle.DynamicProxy;
using Microsoft.Extensions.Logging;

namespace MicrosoftCastle.ConsoleApp
{
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
}
