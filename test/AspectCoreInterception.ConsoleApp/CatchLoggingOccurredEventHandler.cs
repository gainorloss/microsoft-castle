using EventHandlerInterception.Common;

namespace AspectCoreInterception.ConsoleApp
{
    /// <summary>
    /// 
    /// </summary>
    public class CatchLoggingOccurredEventHandler
    : IEventHandler<CatchLoggingOccurredEvent>
    {
        [Idempotent]
        public async virtual Task<bool> HandleAsync(CatchLoggingOccurredEvent @event)
        {
            await Console.Out.WriteLineAsync($"{nameof(CatchLoggingOccurredEventHandler)}处理事件：\t事件【{@event.Id}】@@@@@@发生于【{@event.OccurredOn}】");
            return true;
        }
    }
}
