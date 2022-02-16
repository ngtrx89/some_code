using Common.Models;
using Common.Consts;

namespace Common.Repository.Messages
{
    class UserListRequest: MessageRequest
    {
        public UserListRequest()
        {
            Meta[MessageKeys.Meta.Handler] = MessageHandlerKeys.UserList;
        }
    }
}
