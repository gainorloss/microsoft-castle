using Dora.Interception;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DoraInterceptionAOP.ConsoleApp
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    internal class IdempotentAttribute
        : InterceptorAttribute
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="invocationContext"></param>
        /// <returns></returns>
        public async ValueTask InvokeAsync(InvocationContext invocationContext)
        {
            var logger = invocationContext.InvocationServices.GetRequiredService<ILogger<IdempotentAttribute>>();
            logger.LogInformation("幂等检查");
            await invocationContext.ProceedAsync();
        }
    }
}
