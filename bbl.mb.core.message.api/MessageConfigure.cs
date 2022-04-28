namespace bbl.mb.core.message.api
{
    public class MessageConfigure
    {
        public Uri Uri { get; set; }

        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(10);

        public string GroupId { get; set; }

        public string AutoOffsetReset { get; set; }
    }
}