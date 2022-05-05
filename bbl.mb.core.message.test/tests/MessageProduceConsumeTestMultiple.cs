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
        public async void MultipleProductMessges_MustSuccess()
        {
            // Arrange
            var topic = "testHelloWorld";
            var produceTasks = new Task<MessageActionResult>[30];
            Func<object?, MessageActionResult> produceFunc = (object? param) =>
            {
                var index = param?.ToString();
                var messagePayload = new MessagePayload
                {
                    Name = $"TesstMessage{index}",
                    Payload = $"{DateTime.Now.ToString()} - This is test meesage from task : {index}",
                    Topic = topic
                };

                var result = messageProducer.PostAsync(messagePayload);

                return result.Result;
            };

            // Action
            for (int i = 1; i <= produceTasks.Length; i++)
            {
                produceTasks[i - 1] = Task.Factory.StartNew<MessageActionResult>(produceFunc, i);
            }

            // Assert
            await Task.WhenAll(produceTasks);
            var messageObserver = new MessageConsumeObserver();

            messageConsumer.Subscribe(messageObserver);
            messageConsumer.StartConsume(new MessageConsumerConfigure
            {
                GroupdId = "TestMultipleProductMessages",
                Offset = api.consumer.MessageConsumeOffset.Earliest,
                Topic = topic
            },new CancellationTokenSource(TimeSpan.FromSeconds(20)).Token);

            Assert.True(produceTasks.All(p => p.GetAwaiter().GetResult().IsSuccess));
            Assert.True(messageObserver.Messages.Count() == produceTasks.Length);
        }
    }
}