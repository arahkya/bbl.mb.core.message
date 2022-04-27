namespace bbl.mb.core.message.api
{
    public class MessagePayload<T> : IMessagePayload<T>
        where T : class
    {
        public T Payload { get; internal set; } = default;
    }
}