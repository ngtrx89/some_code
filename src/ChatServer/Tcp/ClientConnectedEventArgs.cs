using System;
using Common.Tcp;

namespace ChatServer.Tcp
{
    internal class ClientConnectedEventArgs : EventArgs
    {
        public TcpClientWorker Client { get; }
        public ClientConnectedEventArgs(TcpClientWorker client)
        {
            Client = client;
        }
    }
}
