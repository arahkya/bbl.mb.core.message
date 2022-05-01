namespace bbl.mb.core.message.api.payload
{
    public class MessagePayload : IMessagePayload
    {
        public string Name { get; set; }
        public string Topic { get; set; }
        public string Payload { get; internal set; } = default;
    }
}