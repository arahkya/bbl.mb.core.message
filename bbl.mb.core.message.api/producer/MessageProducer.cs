using Confluent.Kafka;
using Microsoft.Extensions.Options;
using bbl.mb.core.message.api.configures;
using bbl.mb.core.message.api.payload;

namespace bbl.mb.core.message.api.producer
{
    public class MessageProducer : IMessageProducer
    {
        private readonly MessageConfigure _messageConfigure;

        public MessageProducer(IOptions<MessageConfigure> messageConfigure)
        {
            _messageConfigure = messageConfigure.Value;
        }

        public async Task<MessageActionResult> PostAsync(MessagePayload messagePayload)
        {
            MessageActionResult messageActionResult = new();
            IEnumerable<KeyValuePair<string, string>> producerConfigures = _messageConfigure.ToKeyValuePairs();
            ProducerBuilder<string, string> producerBuilder = new(producerConfigures);

            using (IProducer<string, string> producer = producerBuilder.Build())
            {
                Message<string, string> kafkaMessage = new() { Key = messagePayload.Name, Value = messagePayload.Payload };
                messageActionResult.MessageId = Guid.NewGuid();

                DeliveryResult<string, string> deliveryReport;

                try
                {
                    if (messagePayload.Partition.HasValue)
                    {
                        TopicPartition topicPartition = new(messagePayload.Topic, new Partition(messagePayload.Partition.Value));
                        deliveryReport = await producer.ProduceAsync(topicPartition, kafkaMessage);
                    }
                    else
                    {
                        deliveryReport = await producer.ProduceAsync(messagePayload.Topic, kafkaMessage);
                    }
                    producer.Flush(_messageConfigure.Timeout);

                    messageActionResult.IsSuccess = true;
                }
                catch (ProduceException<string, string> produceException)
                {
                    messageActionResult.Exception = produceException;
                }
                catch (ArgumentException argumentException)
                {
                    messageActionResult.Exception = argumentException;
                }
            }

            return messageActionResult;
        }
    }
}