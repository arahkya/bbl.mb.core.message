namespace bbl.mb.core.message.api.consumer
{
    public class MessageConsumerConfigure
    {
        public string GroupdId { get; set; }
        public string Topic { get; set; }
        public MessageConsumeOffset Offset { get; set; }
    }
}