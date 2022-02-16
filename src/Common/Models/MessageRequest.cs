using Common.Consts;
using System;

namespace Common.Models
{
    public class MessageRequest : Message
    {

        public override Guid MessageId { get; set; } = Guid.NewGuid();
        public MessageRequest()
        {
            Meta.Add(MessageKeys.Meta.Type, MessageType.Request);
            Meta.Add(MessageKeys.Meta.Handler, "Unknown");
        }
    }
}
