using Common.Models;
using Common.Consts;

namespace Common.Repository.Messages
{
    class UserListResponse: MessageResponse
    {
        public UserListResponse()
        {
            Meta[MessageKeys.Meta.Handler] = MessageHandlerKeys.UserList;
            Meta[MessageKeys.Meta.Status] = MessageStatus.Ok;
        }
    }
}
