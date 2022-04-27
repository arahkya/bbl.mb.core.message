using Xunit;
using bbl.mb.core.message.api;
using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace bbl.mb.core.message.test.tests
{
    public class MessageManagerTest
    {
        [Fact]
        public async void PostMessageTest_MustSuccess()
        {
            // Arrange
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new [] { 
                    new KeyValuePair<string,string>("kafka:bootstrap:servers","localhost:9092"),
                    new KeyValuePair<string,string>("kafak:bootstrap:timeout","10") 
                })
                .Build();
            var serviceCollection = new ServiceCollection();
            
            serviceCollection.AddBBLMessageService(configuration);

            var services = serviceCollection.BuildServiceProvider();

            var messageConfigure = new MessageConfigure { Uri = new Uri("http://localhost/core-message") };
            var messagePayload = new MessagePayload<string>
            {
                Topic = "purchases",
                Payload = "This is test message."
            };

            // Action
            var messageMgr = services.GetRequiredService<MessageManager>();
            var postResult = await messageMgr.PostAsync(messagePayload);

            // Assert
            Assert.True(postResult.IsSuccess);
            Assert.NotNull(postResult.MessageId);
            Assert.Null(postResult.Exception);
        }
    }
}