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
                    new KeyValuePair<string,string>("kafka:bootstrap:servers","localhost:9092"),
                    new KeyValuePair<string,string>("kafka:bootstrap:timeout","10")
                })
                .Build();

            serviceCollection.AddBBLMessageService(configuration);

            var services = serviceCollection.BuildServiceProvider();
            this.messageProducer = services.GetRequiredService<IMessageProducer>();
            this.messageConsumer = services.GetRequiredService<IMessageConsumer>();
        }

        [Fact]
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
        public void GetMessageTest_MustSuccess()
        {
            // Arrange
            var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(20));
            var tasks = new Task[2];
            var messagePayload = new MessagePayload
            {
                Name = "TestMessage",
                Topic = "purchases",
                Payload = "This is test message."
            };
            
            var messageConsumeObserver = new MessageConsumeObserver();
            var messageConsumerConfigure = new MessageConsumerConfigure { Topic = "purchases", GroupdId = "test_group_id", Offset = MessageConsumeOffset.Earliest };

            // Action
            messageConsumer.Subscribe(messageConsumeObserver);
            tasks[0] = Task.Run(() =>
            {
                messageConsumer.StartConsume(messageConsumerConfigure, cancellationTokenSource.Token);
            });
            tasks[1] = messageProducer.PostAsync(messagePayload);

            Task.WaitAll(tasks);

            // Assert
            Assert.True(messageConsumeObserver.Messages.Any());
        }
    }
}