using Confluent.Kafka;
using Microsoft.Extensions.Options;
using bbl.mb.core.message.api.configures;

namespace bbl.mb.core.message.api.consumer
{
    public class MessageConsumer : IMessageConsumer
    {
        private List<IObserver<string>> observers = new List<IObserver<string>>();
        private readonly MessageConfigure _messageConfigure;

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
            var bootstrapServerValuePair = new KeyValuePair<string, string>("bootstrap.servers", this._messageConfigure.Uri.ToString());
            var groupIdValuePair = new KeyValuePair<string, string>("group.id", messageConsumerConfigure.GroupdId);
            var offsetResetValuePair = new KeyValuePair<string, string>("auto.offset.reset", messageConsumerConfigure.Offset);
            using (var consumer = new ConsumerBuilder<string, string>(new[]
            {
                bootstrapServerValuePair,
                groupIdValuePair,
                offsetResetValuePair
            }).Build())
            {
                consumer.Subscribe(messageConsumerConfigure.Topic);
                try
                {
                    while (true)
                    {
                        var cr = consumer.Consume(cancellationToken);
                        //Console.WriteLine($"Consumed event from topic {topic} with key {cr.Message.Key,-10} and value {cr.Message.Value}");

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