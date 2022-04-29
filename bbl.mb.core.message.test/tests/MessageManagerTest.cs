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
    public class MessageManagerTest
    {
        [Fact]
        public async void PostMessageTest_MustSuccess()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new[] {
                    new KeyValuePair<string,string>("kafka:bootstrap:servers","localhost:9092"),
                    new KeyValuePair<string,string>("kafka:bootstrap:timeout","10")
                })
                .Build();

            serviceCollection.AddBBLMessageService(configuration);

            var services = serviceCollection.BuildServiceProvider();
            var messagePayload = new MessagePayload<string>
            {
                Topic = "purchases",
                Payload = "This is test message."
            };

            // Action
            var messageMgr = services.GetRequiredService<IMessageProducer>();
            var postResult = await messageMgr.PostAsync(messagePayload);

            // Assert
            Assert.True(postResult.IsSuccess);
            Assert.NotNull(postResult.MessageId);
            Assert.Null(postResult.Exception);
        }

        [Fact]
        public void GetMessageTest_MustSuccess()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new[] {
                    new KeyValuePair<string,string>("kafka:bootstrap:servers","localhost:9092"),
                    new KeyValuePair<string,string>("kafka:bootstrap:timeout","10")
                })
                .Build();

            serviceCollection.AddBBLMessageService(configuration);

            var services = serviceCollection.BuildServiceProvider();
            var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(20));
            var tasks = new Task[2];
            // Producer
            var messagePayload = new MessagePayload<string>
            {
                Topic = "purchases",
                Payload = "This is test message."
            };
            var messageProducer = services.GetRequiredService<IMessageProducer>();
            // Consumer
            var messageConsumer = services.GetRequiredService<IMessageConsumer>();
            var messageConsumeObserver = new MessageConsumeObserver();
            var messageConsumerConfigure = new MessageConsumerConfigure{ Topic = "purchases", GroupdId = "test_group_id", Offset = "earliest" };

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