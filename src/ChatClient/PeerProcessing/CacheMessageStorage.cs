using Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChatClient.PeerProcessing
{
    internal class CacheMessageStorage
    {
        #region Private fields
        private readonly Dictionary<Guid, List<CacheMessage>> _userMessages = new Dictionary<Guid, List<CacheMessage>>();
        #endregion  Private fields

        #region Constructors
        #endregion Constructors

        #region Public props
        #endregion Public props

        #region Public methods
        public void PushMessageFrom(IMessage message)
        {
            lock(_userMessages)
            {
                if (!_userMessages.ContainsKey(message.From.Id))
                    _userMessages.Add(message.From.Id, new List<CacheMessage>());
                _userMessages[message.From.Id].Add(new CacheMessage(message));
            }
            
        }
        public void PushMessageTo(IMessage message)
        {
            lock (_userMessages)
            {
                if (!_userMessages.ContainsKey(message.To.Id))
                    _userMessages.Add(message.To.Id, new List<CacheMessage>());
                _userMessages[message.To.Id].Add(new CacheMessage(message));
            }
        }

        public IEnumerable<CacheMessage> GetMessages(Guid userId)
        {
            lock (_userMessages)
            {
                if (_userMessages.ContainsKey(userId))
                    return _userMessages[userId].OrderBy(msg => msg.Timestamp);
            }
            return new CacheMessage[0];
        }
        #endregion Public methods


        #region Private methods
        #endregion Private methods
    }
}
