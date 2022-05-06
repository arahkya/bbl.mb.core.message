using System.Security;

namespace bbl.mb.core.message.api.configures
{
    public class MessageConfigure
    {
        public string ServerAddress { get; set; }

        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(10);

        public string Protocal { get; set; }

        public FileInfo CAPath { get; set; }

        public FileInfo ClientCertificatePath { get; set; }

        public FileInfo KeyPath { get; set; }
    }
}