namespace EventHandlerInterception.Common
{
    public interface IEvent
    {
        public long Id { get; set; }
        public DateTime OccurredOn { get; set; }
    }
}
