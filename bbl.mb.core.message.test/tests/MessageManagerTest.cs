using Xunit;
using bbl.mb.core.message.api;
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
            var messageMgr = services.GetRequiredService<IMessageManager>();
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
                    new KeyValuePair<string,string>("kafka:bootstrap:timeout","10"),
                    new KeyValuePair<string,string>("kafka:group:id","kafka-dotnet-getting-started"),
                    new KeyValuePair<string,string>("kafka:auto:offset:reset","earliest")
                })
                .Build();

            serviceCollection.AddBBLMessageService(configuration);

            var services = serviceCollection.BuildServiceProvider();
            var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(20));
            var messageConsumer = services.GetRequiredService<IMessageConsumer>();
            var messageConsumeObserver = new MessageConsumeObserver();
            var messagePayload = new MessagePayload<string>
            {
                Topic = "purchases",
                Payload = "This is test message."
            };            
            var messageMgr = services.GetRequiredService<IMessageManager>();
            var tasks = new Task[2];

            // Action
            messageConsumer.Subscribe(messageConsumeObserver);

            tasks[0] = Task.Run(() => {
                messageConsumer.StartConsume("purchases", cancellationTokenSource.Token);
            });
            tasks[1] = messageMgr.PostAsync(messagePayload);

            Task.WaitAll(tasks);

            // Assert
            Assert.True(messageConsumeObserver.Messages.Any());
        }
    }
}