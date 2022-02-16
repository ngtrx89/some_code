using Common.Models;
using Common.Consts;

namespace Common.Repository.Messages
{
    class TextMessageRequest : MessageRequest
    {
        public TextMessageRequest()
        {
            Meta[MessageKeys.Meta.Handler] = MessageHandlerKeys.TextMessage;
        }
    }
}
