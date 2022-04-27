using Xunit;
using bbl.mb.core.message.api;
using System;

namespace bbl.mb.core.message.test.tests
{
    public class MessageManagerTest
    {
        [Fact]
        public async void PostMessageTest_MustSuccess()
        {
            // Arrange
            var messageConfigure = new MessageConfigure { Uri = new Uri("http://localhost/core-message") };
            MessageManager.Instance.Setup(messageConfigure);

            var messagePayload = new MessagePayload<string>
            {
                Payload = "This is test message."
            };

            // Action
            var postResult = await MessageManager.Instance.PostAsync(messagePayload);

            // Assert
            Assert.True(postResult.IsSuccess);
            Assert.NotNull(postResult.MessageId);
        }
    }
}