namespace EventHandlerInterception.Common
{
    public interface IEventHandler
    {
        Task<bool> HandleAsync(IEvent @event);

        bool CanHandle(IEvent @event);
    }

    public interface IEventHandler<T> : IEventHandler
        where T : class, IEvent
    {
        Task<bool> HandleAsync(T @event);

        bool IEventHandler.CanHandle(IEvent @event) => @event.GetType() == typeof(T);//语言特性：默认实现 2024-1-15 10:23:10

        Task<bool> IEventHandler.HandleAsync(IEvent @event) => CanHandle((T)@event) //语言特性：默认实现 2024-1-15 10:23:10
            ? HandleAsync((T)@event)
            : Task.FromResult(false);
    }
}
