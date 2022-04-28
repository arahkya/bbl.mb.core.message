using Confluent.Kafka;
using Microsoft.Extensions.Options;

namespace bbl.mb.core.message.api
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

        public void StartConsume(string topic, CancellationToken cancellationToken)
        {
            var bootstrapServer = new KeyValuePair<string, string>("bootstrap.servers", this._messageConfigure.Uri.ToString());
            var groupId = new KeyValuePair<string, string>("group.id", this._messageConfigure.GroupId.ToString());
            var offsetReset = new KeyValuePair<string, string>("auto.offset.reset", this._messageConfigure.AutoOffsetReset.ToString());
            using (var consumer = new ConsumerBuilder<string, string>(new[] 
            { 
                bootstrapServer,
                groupId,
                offsetReset
            }).Build())
            {
                consumer.Subscribe(topic);
                try
                {
                    while (true)
                    {
                        var cr = consumer.Consume(cancellationToken);
                        //Console.WriteLine($"Consumed event from topic {topic} with key {cr.Message.Key,-10} and value {cr.Message.Value}");

                        foreach (var observer in observers)
                        {
                            observer.OnNext(cr.Message.Value);
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    // Ctrl-C was pressed.
                }
                finally
                {
                    consumer.Close();
                }
            }
        }
    }
}