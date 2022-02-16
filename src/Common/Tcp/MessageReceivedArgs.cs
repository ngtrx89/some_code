using System;
using Common.Interfaces;

namespace Common.Tcp
{
    public class MessageReceivedArgs: EventArgs
    {
        public IMessage Message { get; }
        public MessageReceivedArgs(IMessage message)
        {
            Message = message;
        }
    }
}
