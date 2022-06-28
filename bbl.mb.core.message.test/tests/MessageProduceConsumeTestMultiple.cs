using System;
using System.Threading.Tasks;
using bbl.mb.core.message.api.payload;
using Xunit;
using System.Linq;
using bbl.mb.core.message.api.producer;
using bbl.mb.core.message.test.observers;
using bbl.mb.core.message.api.consumer;
using System.Threading;

namespace bbl.mb.core.message.test.tests
{
    public partial class MessageProduceConsumeTest
    {
        [Fact]
        [Trait("Consumer", "Multiple")]
        public async void MultipleProductMessges_MustSuccess()
        {
            // Arrange
            string? topic = "testHelloWorld";
            Task<MessageActionResult>[]? produceTasks = new Task<MessageActionResult>[30];
            MessageActionResult produceFunc(object? param)
            {
                string? index = param?.ToString();
                MessagePayload? messagePayload = new MessagePayload
                {
                    Name = $"TesstMessage{index}",
                    Payload = $"{DateTime.Now} - This is test meesage from task : {index}",
                    Topic = topic
                };

                Task<MessageActionResult>? result = _messageProducer.PostAsync(messagePayload);

                return result.Result;
            }

            // Action
            for (int i = 1; i <= produceTasks.Length; i++)
            {
                produceTasks[i - 1] = Task.Factory.StartNew(produceFunc, i);
            }

            // Assert
            await Task.WhenAll(produceTasks);
            MessageConsumeObserver? messageObserver = new MessageConsumeObserver();

            _messageConsumer.Subscribe(messageObserver);
            _messageConsumer.StartConsume(new MessageConsumerConfigure
            {
                GroupdId = "TestMultipleProductMessages",
                Offset = api.consumer.MessageConsumeOffset.Earliest,
                Topic = topic
            },new CancellationTokenSource(TimeSpan.FromSeconds(20)).Token);

            Assert.True(produceTasks.All(p => p.GetAwaiter().GetResult().IsSuccess));
            Assert.True(messageObserver.Messages.Count == produceTasks.Length);
        }
    }
}