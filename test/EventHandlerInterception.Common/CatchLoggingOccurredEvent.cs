namespace EventHandlerInterception.Common
{
    public class CatchLoggingOccurredEvent : IEvent
    {
        protected CatchLoggingOccurredEvent()
        {
            OccurredOn = DateTime.Now;
        }

        public CatchLoggingOccurredEvent(long id)
            : this()
        {
            Id = id;
        }

        public long Id { get; set; }
        public DateTime OccurredOn { get; set; }
    }
}
