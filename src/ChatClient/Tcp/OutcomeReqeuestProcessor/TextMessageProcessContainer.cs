using System;
using Common.Models;
using Common.Consts;
using Common.Interfaces;

namespace ChatClient.Tcp.OutcomeReqeuestProcessor
{
    internal class TextMessageProcessContainer : RequestProcessContainerBase
    {
        public override event EventHandler OnRequestProcessed;
        public override event EventHandler OnRequestError;
        #region Private fields
        private IMessage _message;
        #endregion  Private fields

        #region Constructors
        public TextMessageProcessContainer(Guid messageId, IMessage message): base(messageId)
        {
            _message = message;
        }
        #endregion Constructors

        #region Public props
        #endregion Public props

        #region Public methods
        public override void PushResponseMessage(Message message)
        {
            if(message.Meta[MessageKeys.Meta.Status] == MessageStatus.Error)
                OnRequestError?.Invoke(this, new ErrorMessageArgs
                {
                    Status = message.Meta[MessageKeys.Meta.Status],
                    ErrorMessage = message.Payload[MessageKeys.Payload.Message] as string,
                    Handler = MessageHandlerKeys.TextMessage
                });
            else if(message.Meta[MessageKeys.Meta.Status] == MessageStatus.Ok)
                OnRequestProcessed?.Invoke(this, new TextmessageEventArgs
                {
                    Status = message.Meta[MessageKeys.Meta.Status],
                    Message = _message
                });
        }
        #endregion Public methods


        #region Private methods
        #endregion Private methods
    }

    internal class TextmessageEventArgs: EventArgs
    {
        public string Status { get; set; }
        public IMessage Message { get; set; }
    }
}

