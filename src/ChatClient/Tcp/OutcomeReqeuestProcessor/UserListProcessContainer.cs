using Common.Models;
using Common.Consts;
using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;


namespace ChatClient.Tcp.OutcomeReqeuestProcessor
{
    internal class UserListProcessContainer: RequestProcessContainerBase
    {

        public override event EventHandler OnRequestProcessed;
        public override event EventHandler OnRequestError;
        #region Private fields

        private Dictionary<long, object> _userListChunks = new Dictionary<long, object>();
        private long _userListChunksCount = 0;
        #endregion  Private fields

        #region Constructors
        public UserListProcessContainer(Guid messageId): base(messageId)
        {

        }
        #endregion Constructors

        #region Public props
        #endregion Public props

        #region Public methods
        public override void PushResponseMessage(Message message)
        {
            lock (_userListChunks)
            {
                if (message.Meta[MessageKeys.Meta.Status] == MessageStatus.Error)
                    OnRequestError?.Invoke(this, new ErrorMessageArgs
                    {
                        Status = message.Meta[MessageKeys.Meta.Status],
                        ErrorMessage = message.Payload[MessageKeys.Payload.Message] as string,
                        Handler = MessageHandlerKeys.UserList
                    });

                if (message.Payload.ContainsKey(MessageKeys.Payload.UserListChunksCount))
                {
                    _userListChunksCount = (long)message.Payload[MessageKeys.Payload.UserListChunksCount];
                    return;
                }

                if (message.Payload.ContainsKey(MessageKeys.Payload.ChunkNumber))
                    if (!_userListChunks.ContainsKey((long)message.Payload[MessageKeys.Payload.ChunkNumber]))
                        _userListChunks.Add((long)message.Payload[MessageKeys.Payload.ChunkNumber], (message.Payload[MessageKeys.Payload.ChunkData] as JArray).ToObject<List<User>>());

                if (_userListChunks.Count == _userListChunksCount)
                    OnRequestProcessed?.Invoke(this, new UserListEventArgs
                    {
                        UserList = CompileUserList()
                    });
            }
        }
        #endregion Public methods


        #region Private methods
        private List<User> CompileUserList()
        {
            var retVal = new List<User>();
            foreach(var pair in _userListChunks)
            {
                retVal.AddRange(pair.Value as List<User>);
            }
            return retVal;
        }

        #endregion Private methods
    }

    internal class UserListEventArgs: EventArgs
    {
        public List<User> UserList { get; set; }
    }
}
