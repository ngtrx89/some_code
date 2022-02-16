using Common.Consts;
using Common.Models;

namespace Common.Repository.Messages
{
    class GetUserListRequest : MessageRequest
    {
        public GetUserListRequest()
        {
            Meta[MessageKeys.Meta.Handler] = MessageHandlerKeys.UserList;
        }
    }
}
