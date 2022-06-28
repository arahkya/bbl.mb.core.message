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
            KeyValuePair<string, string> bootstrapServer = new("bootstrap.servers", this.ServerAddress);
            KeyValuePair<string, string> bootstrapServerTimeout = new("request.timeout.ms", this.Timeout.TotalMilliseconds.ToString());
            KeyValuePair<string, string> caPath = new("ssl.ca.location", this.CAPath.ToString());
            KeyValuePair<string, string> protocal = new("security.protocol", "SSL");
            KeyValuePair<string, string> clientCertPath = new("ssl.certificate.location", this.ClientCertificatePath.ToString());
            KeyValuePair<string, string> keyPath = new("ssl.key.location", this.KeyPath.ToString());

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