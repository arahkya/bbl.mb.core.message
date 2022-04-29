namespace bbl.mb.core.message.api.payload
{
    public class MessagePayload<T> : IMessagePayload<T>
        where T : class
    {
        public string Topic { get; set; }
        public string Name { get; set; }
        public T Payload { get; internal set; } = default;
    }
}