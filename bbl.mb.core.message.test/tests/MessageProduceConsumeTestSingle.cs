using bbl.mb.core.message.api.consumer;
using bbl.mb.core.message.api.extensions;
using bbl.mb.core.message.api.payload;
using bbl.mb.core.message.api.producer;
using bbl.mb.core.message.test.observers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace bbl.mb.core.message.test.tests
{
    public partial class MessageProduceConsumeTest
    {
        private readonly IMessageProducer _messageProducer;
        private readonly IMessageConsumer _messageConsumer;

        public MessageProduceConsumeTest()
        {
            var serviceCollection = new ServiceCollection();

            string executePath = Environment.CurrentDirectory;
            string solutionPath = Path.GetFullPath("../../../../", executePath);

            IConfigurationRoot? configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new[] {
                    new KeyValuePair<string,string>("kafka:bootstrap:servers", "127.0.0.1:9093"),
                    new KeyValuePair<string,string>("kafka:bootstrap:timeout", "10"),
                    new KeyValuePair<string,string>("kafka:bootstrap:security:keystore:location", Path.Combine(solutionPath, "certs", "ca-root.crt")),
                    new KeyValuePair<string,string>("kafka:bootstrap:security:client-certificate:location", Path.Combine(solutionPath,"certs", "test_client.crt")),
                    new KeyValuePair<string,string>("kafka:bootstrap:security:client-key:location", Path.Combine(solutionPath,"certs", "test_client.key"))
                })
                .Build();

            serviceCollection.AddBBLMessageService(configuration);

            ServiceProvider? services = serviceCollection.BuildServiceProvider();
            _messageProducer = services.GetRequiredService<IMessageProducer>();
            _messageConsumer = services.GetRequiredService<IMessageConsumer>();
        }

        [Fact]
        [Trait("Produce", "Single")]
        public async void PostMessageTest_MustSuccess()
        {
            // Arrange
            MessagePayload? messagePayload = new()
            {
                Name = "TestMessage",
                Topic = "purchases",
                Payload = "This is test message."
            };

            // Action
            MessageActionResult? postResult = await _messageProducer.PostAsync(messagePayload);

            // Assert
            Assert.True(postResult.IsSuccess);
            Assert.NotNull(postResult.MessageId);
            Assert.Null(postResult.Exception);
        }

        [Fact]
        [Trait("Consumer", "Single")]
        public void GetMessageTest_MustSuccess()
        {
            // Arrange
            CancellationTokenSource? cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(20));
            MessagePayload? messagePayload = new()
            {
                Name = "TestMessage",
                Topic = "purchases",
                Payload = "This is test message."
            };

            MessageConsumeObserver? messageConsumeObserver = new();
            MessageConsumerConfigure? messageConsumerConfigure = new() { Topic = "purchases", GroupdId = "test_group_id", Offset = MessageConsumeOffset.Latest };

            _messageConsumer.Subscribe(messageConsumeObserver);

            // Action
            Task? task1 = Task.Run(() =>
            {
                _messageConsumer.StartConsume(messageConsumerConfigure, cancellationTokenSource.Token);
            });

            Task? task2 = Task.Run(async () =>
            {
                int delaySecond = 5; // Increase this if test failed. 
                await Task.Delay(Convert.ToInt16(TimeSpan.FromSeconds(delaySecond).TotalMilliseconds));
                await _messageProducer.PostAsync(messagePayload);
            });

            Task.WaitAll(new[]
            {
                task1,
                task2
            });

            // Assert
            Assert.True(messageConsumeObserver.Messages.Any());
            Assert.Equal(messagePayload.Payload, messageConsumeObserver.Messages.First());
        }
    }
}