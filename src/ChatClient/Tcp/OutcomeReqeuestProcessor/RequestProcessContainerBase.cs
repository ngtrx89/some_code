using System;
using Common.Consts;
using Common.Models;

namespace ChatClient.Tcp.OutcomeReqeuestProcessor
{
    internal class RequestProcessContainerBase
    {
        public virtual event EventHandler OnRequestProcessed;
        public virtual event EventHandler OnRequestError;

        #region Private fields
        #endregion  Private fields

        #region Constructors
        public RequestProcessContainerBase(Guid messageId)
        {
            MessageId = messageId;
        }
        #endregion Constructors

        #region Public props
        public Guid MessageId { get; }
        #endregion Public props

        #region Public methods

        public virtual void PushResponseMessage(Message message)
        {
            throw new NotImplementedException("PushResponseMessage have to be implemented");
        }
        #endregion Public methods


        #region Private methods
        #endregion Private methods
    }
    
    internal class ErrorMessageArgs: EventArgs
    {
        public string Status;
        public string Handler; 
        public string ErrorMessage;
    }
}
