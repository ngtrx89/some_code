using System;
using Common.Models;
using Common.Consts;

namespace ChatClient.Tcp.OutcomeReqeuestProcessor
{
    internal class RegisterUserProcessContainer : RequestProcessContainerBase
    {
        public override event EventHandler OnRequestProcessed;
        public override event EventHandler OnRequestError;
        #region Private fields
        #endregion  Private fields

        #region Constructors
        public RegisterUserProcessContainer(Guid messageId): base(messageId)
        {

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
                    Handler = MessageHandlerKeys.Register
                });
            else if(message.Meta[MessageKeys.Meta.Status] == MessageStatus.Ok)
                OnRequestProcessed?.Invoke(this, new RegisterUserProcessContainerArgs
                {
                    Status = message.Meta[MessageKeys.Meta.Status],
                });
        }
        #endregion Public methods


        #region Private methods
        #endregion Private methods
    }

    internal class RegisterUserProcessContainerArgs : EventArgs
    {
        public string Status { get; set; }
    }
}

