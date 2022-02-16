using Common.Consts;
using Common.Interfaces;
using System;

namespace Common.Repository.Messages
{
    public static class MessageRepository
    {
        public static IMessage GetMessage(ERepositoryMessagesSet message)
        {
            return message switch
            {
                ERepositoryMessagesSet.UnknownError => new UnknownErrorResponse(),
                ERepositoryMessagesSet.Register => new RegisterUserRequest(),
                ERepositoryMessagesSet.OkResponse => new OkResponse(),
                ERepositoryMessagesSet.UserListRequest => new UserListRequest(),
                ERepositoryMessagesSet.UserListResponse => new UserListResponse(),
                ERepositoryMessagesSet.Text => new TextMessageRequest(),
                ERepositoryMessagesSet.Unregister => new UnregisterUserRequest(),
                ERepositoryMessagesSet.HardBit => new HardBitRequest(),
                ERepositoryMessagesSet.ErrorResponse => new ErrorResponse(),
                _ => throw new NotImplementedException($"Message {message} not supported")
            };
        }
    }
}
