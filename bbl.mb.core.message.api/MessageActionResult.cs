namespace bbl.mb.core.message.api
{
    public class MessageActionResult
    {
        public Guid? MessageId { get; internal set; }
        public bool IsSuccess { get; internal set; }
    }
}