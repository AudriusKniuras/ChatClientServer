using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace Server
{
    internal class ConnectionListener : IDisposable
    {
        public ConnectionListener(EndPoint endPoint)
        {
            if (endPoint == null)
                throw new ArgumentNullException(nameof(endPoint));

            _endPoint = endPoint;
            _listenerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _listenerSocket.Bind(_endPoint);
            _listenerSocket.Listen(1);
            _listenerSocket.BeginAccept(OnAccept, null);

            _logger.Info("Started listening for connections.");
        }


        private readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly EndPoint _endPoint;
        private readonly Socket _listenerSocket;
        private readonly IList<String> _usersList;

        internal delegate void ConnectionReceivedDelegate(Socket socket);

        internal event ConnectionReceivedDelegate OnConnectionReceived;

        private void OnAccept(IAsyncResult ar)
        {
            Socket socket = _listenerSocket.EndAccept(ar);
            OnConnectionReceived?.Invoke(socket);
            _listenerSocket.BeginAccept(OnAccept, null);

        }

        #region IDisposable members

        public void Dispose()
        {
            _listenerSocket.Close();
        }

        #endregion
    }
}
