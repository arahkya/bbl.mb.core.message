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
            this._messageConfigure = messageConfigure.Value;
        }

        public async Task<MessageActionResult> PostAsync(MessagePayload messagePayload)
        {
            var messageActionResult = new MessageActionResult();
            var bootstrapServer = new KeyValuePair<string, string>("bootstrap.servers", this._messageConfigure.Uri.ToString());
            var producerBuilder = new ProducerBuilder<string, string>(new[] { bootstrapServer });

            using (var producer = producerBuilder.Build())
            {
                var kafkaMessage = new Message<string, string> { Key = messagePayload.Name, Value = messagePayload.Payload };
                messageActionResult.MessageId = Guid.NewGuid();

                try
                {
                    var deliveryReport = await producer.ProduceAsync(messagePayload.Topic, kafkaMessage);
                    producer.Flush(this._messageConfigure.Timeout);

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