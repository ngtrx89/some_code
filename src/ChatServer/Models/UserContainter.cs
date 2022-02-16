using Common.Interfaces;
using System;
using Common.Tcp;
using Common.Consts;
using Common.Logger;

namespace ChatServer.Models
{
    class UserContainter
    {
        public event EventHandler OnConnectionClosed;
        #region Private fields
        private IUser _user;
        private TcpClientWorker _client;
        #endregion  Private fields
        
        #region Constructors
        public UserContainter(IUser user, TcpClientWorker clientWorker)
        {
            _user = user;
            _client = clientWorker;
            _client.OnMessageReceived += OnMessageReceived;
            _client.OnConnectionClosed += OnClientClosedConnection;
        }   
        #endregion Constructors

        #region Public props
        public Guid Id => _user.Id;
        public string Name => _user.Name;

        public DateTime LastActivityTime { get; private set; } = DateTime.Now;
        #endregion Public props

        #region Public methods
        public void SendMessage(IMessage message) {
            if(_client.Connected)
                _client.SendMessage(message);
        }
        public void StopTcpWorker()
        {
            try
            {
                _client.Stop();
            }
            catch(Exception e)
            {
                Logger.Instance.PushMessage(new LogMessage
                {
                    Type = LoggerMessageTypes.RuntimeError,
                    Message = e.Message
                });
            }
        }
        #endregion Public methods


        #region Private methods
        private void OnClientClosedConnection(object sender, EventArgs e)
        {
            OnConnectionClosed?.Invoke(this, e);
        }
        private void OnMessageReceived(object sender, EventArgs e)
        {
            if (e is MessageReceivedArgs messageArgs)
                if (messageArgs.Message.Meta[MessageKeys.Meta.Handler] != MessageHandlerKeys.HardBit)
                    LastActivityTime = DateTime.Now;
        }
        #endregion Private methods

    }
}
