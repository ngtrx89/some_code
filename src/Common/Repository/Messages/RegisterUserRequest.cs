using Common.Consts;
using Common.Models;

namespace Common.Repository.Messages
{
    public class RegisterUserRequest : MessageRequest
    {
        public RegisterUserRequest()
        {
            Meta[MessageKeys.Meta.Handler] = MessageHandlerKeys.Register;
        }
    }
}
