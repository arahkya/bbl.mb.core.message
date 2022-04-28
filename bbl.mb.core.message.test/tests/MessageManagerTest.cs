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
            var serviceCollection = new ServiceCollection();
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new [] { 
                    new KeyValuePair<string,string>("kafka:bootstrap:servers","localhost:9092"),
                    new KeyValuePair<string,string>("kafak:bootstrap:timeout","10") 
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
    }
}