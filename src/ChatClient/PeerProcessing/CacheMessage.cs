using Common.Consts;
using Common.Interfaces;
using System;

namespace ChatClient.PeerProcessing
{
    class CacheMessage
    {
        #region Private fields

        #endregion  Private fields

        #region Constructors
        public CacheMessage(IMessage message)
        {
            Message = (string)message.Payload[MessageKeys.Payload.Message];
            Sender = message.From;
        }
        #endregion Constructors

        #region Public props
        public DateTime Timestamp { get; } = DateTime.Now;
        public string Message { get; }

        public IUser Sender { get; }
        #endregion Public props

        #region Public methods
        public override string ToString()
        {
            return $"{Timestamp} {Sender}: {Message}";
        }
        #endregion Public methods


        #region Private methods
        #endregion Private methods
    }
}
