using Common.Models;
using Common.Consts;

namespace Common.Repository.Messages
{
    class UnknownErrorResponse: MessageResponse
    {
        public UnknownErrorResponse()
        {
            Payload.Add(MessageKeys.Payload.Message, "Unknown error occured, please contact to support");
            Meta[MessageKeys.Meta.Status] = MessageStatus.Error;
        }
    }
}
