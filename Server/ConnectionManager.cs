using System;
using System.Collections.Generic;

namespace Server
{
    internal class ConnectionManager : IDisposable
    {
        public ConnectionManager(ConnectionListener listener, MessageHandler messageHandler)
        {
            if (listener == null)
                throw new ArgumentNullException(nameof(listener));

            if (messageHandler == null)
                throw new ArgumentNullException(nameof(messageHandler));

            _connections = new List<ConnectionModel>();
            _listener = listener;
            _messageHandler = messageHandler; 
            _listener.OnConnectionReceived += OnConnectionReceivedEventHandler;
        }

        private readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly IList<ConnectionModel> _connections;
        private readonly ConnectionListener _listener;
        private readonly MessageHandler _messageHandler;
        public List<String> UserList = new List<string>();

        private void OnConnectionReceivedEventHandler(System.Net.Sockets.Socket socket)
        {
            if (socket == null)
                throw new ArgumentNullException(nameof(socket));

            _connections.Add(new ConnectionModel(socket, this, _messageHandler));
        }

        public void RemoveConnection(ConnectionModel connection)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));

            _logger.Debug($"Removing connection: '{connection.Id}'");

            connection.Dispose();
            _connections.Remove(connection);
        }

        public void SendAll(Byte[] buffer)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));

            foreach (ConnectionModel connection in _connections)
                connection.Send(buffer);
           
        }

        #region IDisposable members

        public void Dispose()
        {
            _listener?.Dispose();
        }

        #endregion
    }
}
