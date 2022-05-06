using Confluent.Kafka;
using Microsoft.Extensions.Options;
using bbl.mb.core.message.api.configures;

namespace bbl.mb.core.message.api.consumer
{
    public class MessageConsumer : IMessageConsumer
    {
        private List<IObserver<string>> observers = new List<IObserver<string>>();
        private readonly MessageConfigure _messageConfigure;

        public bool IsReady { get; private set; }

        public IDisposable Subscribe(IObserver<string> observer)
        {
            var subscriber = new MessageObserver(this.observers, observer);
            observers.Add(observer);

            return subscriber;
        }

        private class MessageObserver : IDisposable
        {
            private readonly List<IObserver<string>> observers;
            private readonly IObserver<string> observer;

            public MessageObserver(List<IObserver<string>> observers, IObserver<string> observer)
            {
                this.observers = observers;
                this.observer = observer;
            }

            public void Dispose()
            {
                observers.Remove(observer);
            }
        }

        public MessageConsumer(IOptions<MessageConfigure> messageConfigure)
        {
            this._messageConfigure = messageConfigure.Value;
        }

        public void StartConsume(MessageConsumerConfigure messageConsumerConfigure, CancellationToken cancellationToken)
        {
            var bootstrapServerValuePair = new KeyValuePair<string, string>("bootstrap.servers", this._messageConfigure.ServerAddress);
            var bootstrapServerTimeout = new KeyValuePair<string, string>("request.timeout.ms", this._messageConfigure.Timeout.TotalMilliseconds.ToString());
            var groupIdValuePair = new KeyValuePair<string, string>("group.id", messageConsumerConfigure.GroupdId);
            var offsetResetValuePair = new KeyValuePair<string, string>("auto.offset.reset", messageConsumerConfigure.Offset.ToString());                        
            var protocal = new KeyValuePair<string, string>("security.protocol", "SSL");
            var caPath = new KeyValuePair<string,string>("ssl.ca.location", this._messageConfigure.CAPath.ToString());
            var clientCertPath = new KeyValuePair<string, string>("ssl.certificate.location", this._messageConfigure.ClientCertificatePath.ToString());            
            var keyPath = new KeyValuePair<string, string>("ssl.key.location", this._messageConfigure.KeyPath.ToString());
            var consumerBuilder = new ConsumerBuilder<string, string>(new[]
            {
                bootstrapServerValuePair,
                bootstrapServerTimeout,
                groupIdValuePair,
                offsetResetValuePair,
                protocal,
                caPath,
                clientCertPath,
                keyPath
            });

            using (var consumer = consumerBuilder.Build())
            {
                consumer.Subscribe(messageConsumerConfigure.Topic);
                try
                {
                    while (true)
                    {                                           
                        var cr = consumer.Consume(cancellationToken);

                        foreach (IMessageConsumeObserver item in observers)
                        {
                            item.Configure = messageConsumerConfigure;
                            item.OnNext(cr.Message.Value);
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    foreach (var item in observers)
                    {
                        item.OnCompleted();
                    }
                }
                catch (Exception ex)
                {
                    foreach (var item in observers)
                    {
                        item.OnError(ex);
                    }
                }
                finally
                {
                    consumer.Close();
                }
            }
        }
    }
}