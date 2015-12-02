using System;
using System.Net;
using System.Net.Sockets;
using MutualStuff;

namespace Client
{
    internal class ConnectionConnector : IDisposable
    {
        public ConnectionConnector(EndPoint endPoint, string username)
        {
            if (endPoint == null)
                throw new ArgumentNullException(nameof(endPoint));
            _username = username;
            _buffer = new byte[1024];
            _endPoint = endPoint;
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _socket.Connect(endPoint);

            _logger.Debug("Successfully connected to the server.");
            _socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, OnReceive, null);
        }

        private readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly Socket _socket;
        private readonly EndPoint _endPoint;
        private readonly byte[] _buffer;
        private readonly string _username;

        internal delegate void MessageReceivedDelegate(MessageData messageData);

        internal event MessageReceivedDelegate OnMessageReceived;


        private void OnReceive(IAsyncResult ar)
        {
            try
            {
                Int32 bytesRead = _socket.EndReceive(ar);
                if (bytesRead == 0)
                    return;


                MessageData msgData = new MessageData(_buffer);

                OnMessageReceived?.Invoke(msgData);

                _socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, OnReceive, null);
            }
            catch (ObjectDisposedException) { }
            catch (SocketException)
            {
                _socket?.Shutdown(SocketShutdown.Both);
                _socket?.Close();
                throw;
            }
        }

        public void Send(Byte[] buffer, MessageType messageType)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));

            MessageData msgData = new MessageData
            {
                UserInfo =
                {
                    MessageType = messageType,
                    Username = _username,
                    Message = buffer.ToString()
                }
            };
            _socket.Send(msgData.ToByte());
        }

        public void Send(Byte[] buffer)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));

            _socket.Send(buffer);
        }

        public void Send(String text, MessageType messageType)
        {
            if (String.IsNullOrEmpty(text))
                throw new ArgumentNullException(nameof(text));

            MessageData msgData = new MessageData
            {
                UserInfo =
                {
                    MessageType = messageType,
                    Username = _username,
                    Message = text
                }
            };
            _socket.Send(msgData.ToByte());
        }

        #region IDisposable members

        public void Dispose()
        {
            _socket?.Shutdown((SocketShutdown.Both));
            _socket?.Close();
        }

        #endregion
    }
}
