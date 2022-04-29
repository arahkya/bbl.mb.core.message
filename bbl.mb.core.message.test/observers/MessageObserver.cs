using System;
using System.Collections.Generic;
using bbl.mb.core.message.api.configures;
using bbl.mb.core.message.api.consumer;

namespace bbl.mb.core.message.test.observers
{
    public class MessageConsumeObserver : IMessageConsumeObserver
    {
        public MessageConsumerConfigure Configure { get; set; } = new MessageConsumerConfigure();
        public List<string> Messages = new List<string>();


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