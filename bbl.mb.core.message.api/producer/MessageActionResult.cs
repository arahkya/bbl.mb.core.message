namespace bbl.mb.core.message.api.producer
{
    #nullable enable
    public class MessageActionResult
    {
        public Guid? MessageId { get; internal set; }
        public bool IsSuccess { get; internal set; }
        public Exception? Exception { get; internal set; }
    }
}