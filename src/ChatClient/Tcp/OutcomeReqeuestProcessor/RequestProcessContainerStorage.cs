using System;
using System.Collections.Generic;
using System.Linq;
using Common.Consts;
using Common.Models;

namespace ChatClient.Tcp.OutcomeReqeuestProcessor
{
    public class RequestProcessContainerStorage
    {

        #region Private fields
        private readonly List<RequestProcessContainerBase> _requests = new List<RequestProcessContainerBase>();
        #endregion  Private fields

        #region Constructors
        #endregion Constructors

        #region Public props
        #endregion Public props

        #region Public methods

        public void CreateNewRequestProcessor(MessageRequest request, EventHandler successEventHandler = null, EventHandler errorEventHandler = null)
        {
            lock (_requests)
                if (!IsRequestRegiseterd(request.MessageId))
                {
                    RequestProcessContainerBase container;
                    switch (request.Meta[MessageKeys.Meta.Handler])
                    {
                        case MessageHandlerKeys.TextMessage: { container = new TextMessageProcessContainer(request.MessageId, request); break; }
                        case MessageHandlerKeys.UserList: { container = new UserListProcessContainer(request.MessageId); break; }
                        case MessageHandlerKeys.Register: { container = new RegisterUserProcessContainer(request.MessageId); break; }
                        default: { throw new ArgumentOutOfRangeException($"Handler {request.Meta[MessageKeys.Meta.Handler]} doesn't supprot"); }
                    }
                    container.OnRequestProcessed += successEventHandler;
                    container.OnRequestError += errorEventHandler;
                    container.OnRequestProcessed += OnRequestProcessedHandler;
                    container.OnRequestError += OnRequestProcessedHandler;
                    _requests.Add(container);
                }
        }

        public void PushResponseMessage(Message response)
        {
            if (null == response)
                return;

            lock (_requests)
                if (IsRequestRegiseterd(response.MessageId))
                    _requests.Find(r => r.MessageId == response.MessageId).PushResponseMessage(response);
        }

        public bool IsRequestRegiseterd(Guid messageId) => _requests.Count(u => u.MessageId == messageId) > 0;
        #endregion Public methods


        #region Private methods
        private void OnRequestProcessedHandler(object sender, EventArgs e)
        {
            lock (_requests)
                _requests.Remove(_requests.Find(r => r.MessageId == (sender as RequestProcessContainerBase).MessageId));
        }
        #endregion Private methods
    }
}
