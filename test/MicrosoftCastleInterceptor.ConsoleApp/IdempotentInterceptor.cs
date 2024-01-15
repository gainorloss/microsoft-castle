using Castle.DynamicProxy;
using Microsoft.Extensions.Logging;

namespace MicrosoftCastleInterceptor.ConsoleApp
{
    internal class IdempotentInterceptor
        : InterceptorBase
    {
        private readonly ILogger<IdempotentInterceptor> _logger;

        public IdempotentInterceptor(ILogger<IdempotentInterceptor> logger)
        {
            _logger = logger;
        }

        protected override async ValueTask InterceptAsync(IAsyncInvocation invocation)
        {
            _logger.LogInformation("幂等检查");
            await invocation.ProceedAsync();
    }
    }
}
