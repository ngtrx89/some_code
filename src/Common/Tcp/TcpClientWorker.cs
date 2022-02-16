using System;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;
using System.Threading;
using System.Threading.Tasks;
using Common.Models;
using Common.Interfaces;
using Common.Repository.Messages;

namespace Common.Tcp
{
    public class TcpClientWorker
    {
        const int HardBitPollingTimeout = 5000;//ms
        const int DataInfoBuffSize = sizeof(int);
        const int DataBuffSize1KB = 1024;

        public event EventHandler OnMessageReceived;
        public event EventHandler OnConnectionClosed;

        #region Private fields
        private TcpClient _client;
        private bool _isWorkerStarted = false;
        private bool _isHardBitStarted = false;
        private readonly object _sendLock = new object();
        private readonly object _receiveLock = new object();
        #endregion  Private fields

        #region Constructors
        public TcpClientWorker(TcpClient client)
        {
            _client = client;
            _client.NoDelay = true;
        }
        #endregion Constructors

        #region Public props
        public bool Connected => _client.Connected;
        #endregion Public props

        #region Public methods
        public async void Start()
        {
            _isWorkerStarted = true;
            _isHardBitStarted = true;
            var ch = Task.Run(ConnectionHandler);
            var hbp = Task.Run(HardBitPolling);
            await ch;
            await hbp;
        }

        public void SendMessage(IMessage message)
        {
            lock (_sendLock)
            {
                try
                {
                    var bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message as Message));
                    _client?.Client?.Send(BitConverter.GetBytes((int)bytes.Length));
                    _client?.Client?.Send(bytes);
                }
                catch (SocketException se)
                {
                    if (se.SocketErrorCode == SocketError.ConnectionAborted)
                        Stop();
                    Logger.Logger.Instance.PushMessage(new Logger.LogMessage
                    {
                        Type = Consts.LoggerMessageTypes.RuntimeError,
                        Message = se.Message
                    });
                };
            }
        }
        public void Stop()
        {
            _isWorkerStarted = false;
            _isHardBitStarted = false;
            if (_client != null && _client.Connected)
                _client.Close();
        }

        ~TcpClientWorker()
        {
            Stop();
        }
        #endregion Public methods


        #region Private methods
        private void ConnectionHandler()
        {
            var bytes = new byte[DataBuffSize1KB];
            var infoBytes = new byte[DataInfoBuffSize];
            int receivedCount;

            while (_isWorkerStarted)
            {
                try
                {
                    if(!_client.Connected)
                    {
                        Stop();
                        continue;
                    }
                    if(_client.Available < DataInfoBuffSize)
                    {
                        Thread.Sleep(10);
                        continue;
                    }
                    _client.Client.Receive(infoBytes, 0, DataInfoBuffSize, SocketFlags.None);
                    var buffSize = BitConverter.ToInt32(infoBytes);
                    receivedCount = 0;
                    do
                    {
                        if (!_isWorkerStarted || !_client.Connected)
                            break;
                        if(_client.Available < buffSize)
                        {
                            Thread.Sleep(10);
                            continue;
                        }
                        receivedCount = _client.Client.Receive(bytes, 0, buffSize, SocketFlags.None);
                        OnMessageReceived?.Invoke(this, new MessageReceivedArgs(JsonConvert.DeserializeObject<Message>(Encoding.UTF8.GetString(bytes[..buffSize]))));
                    } while (receivedCount != buffSize);
                }
                catch (Exception e)
                {
                    Logger.Logger.Instance.PushMessage(new Logger.LogMessage
                    {
                        Type = Consts.LoggerMessageTypes.RuntimeError,
                        Message = e.Message
                    });
                }
            }
            OnConnectionClosed?.Invoke(this, new EventArgs());
        }

        private void HardBitPolling()
        {
            while (_isHardBitStarted)
            {
                try
                {
                    SendMessage(MessageRepository.GetMessage(Consts.ERepositoryMessagesSet.HardBit));
                    Thread.Sleep(HardBitPollingTimeout);
                }
                catch(SocketException se)
                {
                    if(se.SocketErrorCode == SocketError.ConnectionAborted)
                        Stop();
                }
                catch (Exception e)
                {
                    Logger.Logger.Instance.PushMessage(new Logger.LogMessage
                    {
                        Type = Consts.LoggerMessageTypes.RuntimeError,
                        Message = e.Message
                    });
                }
            }
        }
        #endregion Private methods
    }
}
