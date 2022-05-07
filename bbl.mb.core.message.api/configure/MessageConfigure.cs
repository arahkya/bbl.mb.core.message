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

        internal IEnumerable<KeyValuePair<string, string>> ToKeyValuePairs()
        {
            var bootstrapServer = new KeyValuePair<string, string>("bootstrap.servers", this.ServerAddress);
            var bootstrapServerTimeout = new KeyValuePair<string, string>("request.timeout.ms", this.Timeout.TotalMilliseconds.ToString());
            var caPath = new KeyValuePair<string, string>("ssl.ca.location", this.CAPath.ToString());
            var protocal = new KeyValuePair<string, string>("security.protocol", "SSL");
            var clientCertPath = new KeyValuePair<string, string>("ssl.certificate.location", this.ClientCertificatePath.ToString());
            var keyPath = new KeyValuePair<string, string>("ssl.key.location", this.KeyPath.ToString());

            return new[] 
            {
                bootstrapServer,
                bootstrapServerTimeout,
                caPath,
                protocal,
                clientCertPath,
                keyPath
            };
        }
    }
}