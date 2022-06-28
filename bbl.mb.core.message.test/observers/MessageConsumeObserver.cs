using bbl.mb.core.message.api.consumer;
using System;
using System.Collections.Generic;

namespace bbl.mb.core.message.test.observers
{
    public class MessageConsumeObserver : IMessageConsumeObserver
    {
        public MessageConsumerConfigure Configure { get; set; } = new MessageConsumerConfigure();
        public List<string> Messages = new();

        public void OnCompleted()
        {

        }

        public void OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        public void OnNext(string value)
        {
            Messages.Add(value);
        }
    }
}