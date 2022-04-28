using System;
using System.Collections.Generic;

namespace bbl.mb.core.message.test.observers
{
    public class MessageConsumeObserver : IObserver<string>
    {
        public List<string> Messages = new List<string>();

        public void OnCompleted()
        {
            throw new NotImplementedException();
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