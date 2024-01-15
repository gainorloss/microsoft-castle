using EventHandlerInterception.Common;
using Microsoft.Extensions.Logging;

namespace DoraInterceptionAOP.ConsoleApp
{
    /// <summary>
    /// 
    /// </summary>
    public class CatchLoggingOccurredEventHandler
    : IEventHandler<CatchLoggingOccurredEvent>
    {
        private readonly ILogger<CatchLoggingOccurredEventHandler> _logger;

        public CatchLoggingOccurredEventHandler(ILogger<CatchLoggingOccurredEventHandler> logger)//在该处依赖注入 dora报错 2024-1-15 11:08:37 https://github.com/jiangjinnan/Dora/pull/13
        {
            _logger = logger;
        }

        [Idempotent]
        public async virtual Task<bool> HandleAsync(CatchLoggingOccurredEvent message)
        {
            await Console.Out.WriteLineAsync($"{nameof(CatchLoggingOccurredEventHandler)}处理事件：\t事件【{message.Id}】@@@@@@发生于【{message.OccurredOn}】");
            return true;
        }
    }
}
