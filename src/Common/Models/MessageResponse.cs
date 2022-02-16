using Common.Consts;

namespace Common.Models
{
    public class MessageResponse : Message
    {
        public MessageResponse()
        {
            Meta.Add(MessageKeys.Meta.Type, MessageType.Response);
            Meta.Add(MessageKeys.Meta.Status, MessageStatus.Ok);
        }
    }
}
