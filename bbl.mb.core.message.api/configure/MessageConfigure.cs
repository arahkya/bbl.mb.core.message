using System.Security;

namespace bbl.mb.core.message.api.configures
{
    public class MessageConfigure
    {
        public string ServerAddress { get; set; }

        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(10);

        public string Protocal { get; set; }

        public DirectoryInfo CAPath { get; set; }
    }        
}