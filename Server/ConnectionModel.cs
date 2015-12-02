using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using MutualStuff;
using Encryption;

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

        public ConnectionInformation ConnectionInformation;


        private void OnReceive(IAsyncResult ar)
        {
            try
            {
                Int32 bytesRead = _socket.EndReceive(ar);
                if (bytesRead == 0)
                    return;
                //Program.ConnectionManager.messageQueue.Add(_buffer);
                //while (Program.ConnectionManager.messageQueue.Count != 0)
                //{
                //_messageHandler.HandleMessage(this, new MessageData(Program.ConnectionManager.messageQueue[0]));
                //}

                /*using (var networkStream = new NetworkStream(_socket))
                using (var bufferedStream = new BufferedStream(networkStream))
                {
                    while(true)
                    {
                        if (!TryReadExact(bufferedStream, _buffer, 0, 1))
                        {
                            break;
                        }
                        int msgLen = _buffer[0];
                        if (!TryReadExact(bufferedStream, _buffer, 1, msgLen))
                        {
                            break;
                        }
                        _messageHandler.HandleMessage(this, new MessageData(_buffer));
                    }
                }*/
                _messageHandler.HandleMessage(this, new MessageData(_buffer));

                    

                _socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, OnReceive, null);
            }
            catch(SocketException sex)
            {
                _manager.RemoveConnection(this);
            }
        }

        private static bool TryReadExact(Stream stream, byte[] buffer, int offset, int count)
        {
            int bytesRead;
            while (count > 0 && ((bytesRead = stream.Read(buffer, offset, count)) > 0))
            {
                offset += bytesRead;
                count -= bytesRead;
            }
            return count == 0;
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
