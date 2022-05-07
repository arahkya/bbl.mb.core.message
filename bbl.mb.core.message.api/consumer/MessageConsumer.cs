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
            var consumerConfigures = new List<KeyValuePair<string,string>>(this._messageConfigure.ToKeyValuePairs());
            consumerConfigures.Add(new KeyValuePair<string, string>("group.id", messageConsumerConfigure.GroupdId));
            consumerConfigures.Add(new KeyValuePair<string, string>("auto.offset.reset", messageConsumerConfigure.Offset.ToString()));

            var consumerBuilder = new ConsumerBuilder<string, string>(consumerConfigures);

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