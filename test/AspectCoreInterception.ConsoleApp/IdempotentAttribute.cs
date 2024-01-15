using AspectCore.DynamicProxy;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AspectCoreInterception.ConsoleApp
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    internal class IdempotentAttribute
        : AbstractInterceptorAttribute
    {
        public override async Task Invoke(AspectContext context, AspectDelegate next)
        {
            var logger = context.ServiceProvider.GetRequiredService<ILogger<IdempotentAttribute>>();
            logger.LogInformation("幂等检查");
            await next(context);
        }
    }
}
