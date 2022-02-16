using Common.Models;
using Common.Consts;

namespace Common.Repository.Messages
{
    class OkResponse: MessageResponse
    {
        public OkResponse()
        {
            Payload[MessageKeys.Payload.Message] = "Message delivered";
            Meta[MessageKeys.Meta.Status] = MessageStatus.Ok;
        }
    }
}
