using Xunit;
using bbl.mb.core.message.api.extensions;
using bbl.mb.core.message.api.consumer;
using bbl.mb.core.message.api.producer;
using bbl.mb.core.message.api.payload;
using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading;
using bbl.mb.core.message.test.observers;
using System.Threading.Tasks;
using System.ComponentModel;

namespace bbl.mb.core.message.test.tests
{
    public partial class MessageProduceConsumeTest
    {
        private readonly IMessageProducer messageProducer;
        private readonly IMessageConsumer messageConsumer;

        public MessageProduceConsumeTest()
        {
            var serviceCollection = new ServiceCollection();
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new[] {
                    new KeyValuePair<string,string>("kafka:bootstrap:servers", "127.0.0.1:9093"),
                    new KeyValuePair<string,string>("kafka:bootstrap:timeout", "10"),                    
                    new KeyValuePair<string,string>("kafka:bootstrap:security:keystore:location", "/Users/arrakyambupah/Sources/bbl.mb.core.message/certs/ca-root.crt"),
                    new KeyValuePair<string,string>("kafka:bootstrap:security:client-certificate:location", "/Users/arrakyambupah/Sources/bbl.mb.core.message/certs/test_client.crt"),
                    new KeyValuePair<string,string>("kafka:bootstrap:security:client-key:location", "/Users/arrakyambupah/Sources/bbl.mb.core.message/certs/test_client.key")
                })
                .Build();

            serviceCollection.AddBBLMessageService(configuration);

            var services = serviceCollection.BuildServiceProvider();
            this.messageProducer = services.GetRequiredService<IMessageProducer>();
            this.messageConsumer = services.GetRequiredService<IMessageConsumer>();
        }

        [Fact]
        [Trait("Produce", "Single")]
        public async void PostMessageTest_MustSuccess()
        {
            // Arrange
            var messagePayload = new MessagePayload
            {
                Name = "TestMessage",
                Topic = "purchases",
                Payload = "This is test message."
            };

            // Action
            var postResult = await messageProducer.PostAsync(messagePayload);

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
            var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(20));
            var messagePayload = new MessagePayload
            {
                Name = "TestMessage",
                Topic = "purchases",
                Payload = "This is test message."
            };

            var messageConsumeObserver = new MessageConsumeObserver();
            var messageConsumerConfigure = new MessageConsumerConfigure { Topic = "purchases", GroupdId = "test_group_id", Offset = MessageConsumeOffset.Latest };

            messageConsumer.Subscribe(messageConsumeObserver);

            // Action
            var task1 = Task.Run(() =>
            {
                messageConsumer.StartConsume(messageConsumerConfigure, cancellationTokenSource.Token);
            });

            var task2 = Task.Run(async () => {
                int delaySecond = 5; // Increase this if test failed. 
                await Task.Delay(Convert.ToInt16(TimeSpan.FromSeconds(delaySecond).TotalMilliseconds));
                await messageProducer.PostAsync(messagePayload);
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