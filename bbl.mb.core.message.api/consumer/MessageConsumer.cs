using Confluent.Kafka;
using Microsoft.Extensions.Options;
using bbl.mb.core.message.api.configures;

namespace bbl.mb.core.message.api.consumer
{
    public class MessageConsumer : IMessageConsumer
    {
        private readonly MessageConfigure _messageConfigure;

        #region Observer
        private readonly List<IObserver<string>> _observers = new();

        public IDisposable Subscribe(IObserver<string> observer)
        {
            MessageObserver subscriber = new(_observers, observer);
            _observers.Add(observer);

            return subscriber;
        }

        private sealed class MessageObserver : IDisposable
        {
            private readonly List<IObserver<string>> _observers;
            private readonly IObserver<string> _observer;

            public MessageObserver(List<IObserver<string>> observers, IObserver<string> observer)
            {
                _observers = observers;
                _observer = observer;
            }

            public void Dispose()
            {   
                _observers.Remove(_observer);
                GC.SuppressFinalize(this);
            }

            ~MessageObserver()
            {
                Dispose();
            }
        }
        #endregion

        public MessageConsumer(IOptions<MessageConfigure> messageConfigure)
        {
            _messageConfigure = messageConfigure.Value;
        }

        public void StartConsume(MessageConsumerConfigure messageConsumerConfigure, CancellationToken cancellationToken)
        {
            List<KeyValuePair<string, string>> consumerConfigures = new(_messageConfigure.ToKeyValuePairs())
            {
                new KeyValuePair<string, string>("group.id", messageConsumerConfigure.GroupdId),
                new KeyValuePair<string, string>("auto.offset.reset", messageConsumerConfigure.Offset.ToString())
            };

            ConsumerBuilder<string, string> consumerBuilder = new(consumerConfigures);

            using IConsumer<string, string> consumer = consumerBuilder.Build();
            consumer.Subscribe(messageConsumerConfigure.Topic);

            try
            {
                if (messageConsumerConfigure.Partition.HasValue)
                {
                    TopicPartition partition = new(messageConsumerConfigure.Topic, new Partition(messageConsumerConfigure.Partition.Value));
                    consumer.Assign(partition);
                }

                while (true)
                {
                    // Thread will block here until it has new message to consume.
                    ConsumeResult<string, string> cr = consumer.Consume(cancellationToken);

                    // After got new message MessageConsumer will publish the message to each observers.
                    foreach (IMessageConsumeObserver item in _observers.Cast<IMessageConsumeObserver>())
                    {
                        item.Configure = messageConsumerConfigure;
                        item.OnNext(cr.Message.Value);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                foreach (IObserver<string> item in _observers)
                {
                    item.OnCompleted();
                }
            }
            catch (Exception ex)
            {
                foreach (IObserver<string> item in _observers)
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