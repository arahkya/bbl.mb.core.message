using Confluent.Kafka;
using Microsoft.Extensions.Options;

namespace bbl.mb.core.message.api
{
    public class MessageManager
    {
        private readonly MessageConfigure _messageConfigure;

        public MessageManager(IOptions<MessageConfigure> messageConfigure)
        {
            this._messageConfigure = messageConfigure.Value;
        }

        public async Task<MessageActionResult> PostAsync<T>(MessagePayload<T> messagePayload) where T : class
        {
            var messageActionResult = new MessageActionResult();
            var bootstrapServer = new KeyValuePair<string, string>("bootstrap.servers", this._messageConfigure.Uri.ToString());
            var producerBuilder = new ProducerBuilder<string, T>(new[] { bootstrapServer });

            using (var producer = producerBuilder.Build())
            {
                var kafkaMessage = new Message<string, T> { Key = messagePayload.Name, Value = messagePayload.Payload };
                messageActionResult.MessageId = Guid.NewGuid();

                try
                {
                    var deliveryReport = await producer.ProduceAsync(messagePayload.Topic, kafkaMessage);
                    messageActionResult.IsSuccess = true;

                    producer.Flush(this._messageConfigure.Timeout);
                }
                catch (ProduceException<string, T> produceException)
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