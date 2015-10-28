using System;
using System.Net.Sockets;
using System.Text;
using MutualStuff;

namespace Server
{
    internal class ConnectionModel : IDisposable
    {
        public ConnectionModel(Socket socket, ConnectionManager manager, MessageHandler messageHandler)
        {
            if (socket == null)
                throw new ArgumentNullException(nameof(socket));

            if (messageHandler == null)
                throw new ArgumentNullException(nameof(messageHandler));

            Id = Guid.NewGuid();
            _buffer = new Byte[1024];
            _socket = socket;
            _manager = manager;
            _messageHandler = messageHandler;

            _logger.Debug($"New ConnectionModel created. Id: '{Id}'");

            _socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, OnReceive, null);
        }

        private readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly ConnectionManager _manager;
        private readonly MessageHandler _messageHandler;
        private readonly Socket _socket;
        private Byte[] _buffer;
        /// <summary>
        /// Unique identifier
        /// </summary>
        public Guid Id { get; }


        private void OnReceive(IAsyncResult ar)
        {
            try
            {
                Int32 bytesRead = _socket.EndReceive(ar);
                if (bytesRead == 0)
                    return;

                _messageHandler.HandleMessage(this, new MessageData(_buffer));

                _socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, OnReceive, null);
            }
            catch(SocketException sex)
            {
                _manager.RemoveConnection(this);
            }
        }

        public void Send(Byte[] buffer)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));

            _socket.Send(buffer);
        }
        /*
        public void Send(String text)
        {
            if (String.IsNullOrEmpty(text))
                throw new ArgumentNullException(nameof(text));
            MessageData msgData = new MessageData();
            msgData.UserInfo.MessageType = MessageType.Message;
            msgData.UserInfo.Message = text;

            _socket.Send(msgData.ToByte());
        }
        */
        #region IDisposable members

        public void Dispose()
        {
            _socket?.Close();
        }

        #endregion
    }
}
