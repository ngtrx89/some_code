using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Common.Tcp;
using Common.Logger;
using Common.Consts;

namespace ChatServer.Tcp
{
    sealed class TcpServer : IDisposable
    {

        public const int ListenerTimeout = 1000;
        public const int SouredConnectionWorkerTimeout = 10000;

        public event EventHandler OnClientConnected;
        public event EventHandler OnServerStartError;

        #region Private fields
        private TcpListener _server;
        private bool _isServerStarted = false;
        private readonly List<TcpClientWorker> _tcpClients = new List<TcpClientWorker>();
        private readonly object _clientLock = new object();
        #endregion  Private fields

        #region Constructors
        public TcpServer(string host, int port) {
            Host = host;
            Port = port;
            var localAddr = IPAddress.Parse(Host);
            _server = new TcpListener(localAddr, Port);
        }
        #endregion Constructors

        #region Public props
        public string Host { get; private set; }
        public int Port { get; private set; }

        public bool IsStarted => _isServerStarted;
        #endregion Public props

        #region Public methods
        public async void Start()
        {
            try
            {
                _isServerStarted = true;
                var serverLoop = Task.Run(ListenerWorker);
                var souredClientsLoop = Task.Run(SouredConnectionsWorker);
                await serverLoop;
                await souredClientsLoop;
            }
            catch(Exception e)
            {
                Logger.Instance.PushMessage(new LogMessage
                {
                    Type = LoggerMessageTypes.RuntimeError,
                    Message = e.Message
                });
                OnServerStartError?.Invoke(this, new EventArgs());
            }
            
        }

        public void Stop()
        {
            try
            {
                if (!_isServerStarted)
                    return;
                _isServerStarted = false;
                _server?.Stop();
            }
            catch (Exception e)
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
        private void ListenerWorker()
        {
            _server.Start();
            while (_isServerStarted)
            {
                try
                {
                    var client = new TcpClientWorker(_server.AcceptTcpClient());
                    _tcpClients?.Add(client);
                    OnClientConnected?.Invoke(null, new ClientConnectedEventArgs(client));
                }
                catch (Exception e)
                {
                    Logger.Instance.PushMessage(new LogMessage
                    {
                        Type = LoggerMessageTypes.RuntimeError,
                        Message = e.Message
                    });
                    Thread.Sleep(ListenerTimeout);
                }
            }
            lock (_clientLock)
                try
                {
                    _tcpClients.AsParallel().ForAll((client) => {
                        try
                        {
                            client?.Stop();

                        }
                        catch (Exception e)
                        {
                            Logger.Instance.PushMessage(new LogMessage
                            {
                                Type = LoggerMessageTypes.RuntimeError,
                                Message = e.Message
                            });
                        }
                        finally
                        {
                            if (null != client)
                                _tcpClients.Remove(client);
                        }
                    });
                }
                catch (Exception e)
                {
                    Logger.Instance.PushMessage(new LogMessage
                    {
                        Type = LoggerMessageTypes.RuntimeError,
                        Message = e.Message
                    });
                }
            
        }

        private void SouredConnectionsWorker()
        {
            while (_isServerStarted)
            {
                try
                {
                    lock (_clientLock)
                        _tcpClients.AsParallel().ForAll((client) => {
                        try
                        {
                            if (null != client && !client.Connected)
                                _tcpClients?.Remove(client);
                        }
                        catch (Exception e)
                        {
                            Logger.Instance.PushMessage(new LogMessage
                            {
                                Type = LoggerMessageTypes.RuntimeError,
                                Message = e.Message
                            });
                        }
                    });
                }
                catch (Exception e)
                {
                    Logger.Instance.PushMessage(new LogMessage
                    {
                        Type = LoggerMessageTypes.RuntimeError,
                        Message = e.Message
                    });
                }
                finally
                {
                    Thread.Sleep(SouredConnectionWorkerTimeout);
                }
            }
        }

        public void Dispose()
        {
            Stop();
        }
        #endregion Private methods
    }
}
