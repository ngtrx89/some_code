using Common.Models;
using Common.Consts;

namespace Common.Repository.Messages
{

    internal class UnregisterUserRequest: MessageRequest
    {
        public UnregisterUserRequest()
        {
            Meta[MessageKeys.Meta.Handler] = MessageHandlerKeys.Unregister;
        }
    }
}
