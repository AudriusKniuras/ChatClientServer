using System;
using MutualStuff;

namespace Server
{
    internal class MessageHandler
    {
        private readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        public void HandleMessage(ConnectionModel connection, MessageData messageData)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));

            MessageData messageToSend = new MessageData();

            switch(messageData.UserInfo.MessageType)
            {
                case MessageType.Connect:
                    messageToSend.UserInfo.MessageType = MessageType.Connect;
                    messageToSend.UserInfo.Username = messageData.UserInfo.Username;
                    Program.ConnectionManager.UserList.Add(messageData.UserInfo.Username);
                    messageToSend.UserInfo.Message = ">>> " + messageData.UserInfo.Username + " connected";
                    break;

                case MessageType.Disconnect:
                    messageToSend.UserInfo.MessageType = MessageType.Disconnect;
                    Program.ConnectionManager.UserList.Remove(messageData.UserInfo.Username);
                    messageToSend.UserInfo.Message = "<<< " + messageData.UserInfo.Username + " disconnected";
                    break;

                case MessageType.Message:
                    messageToSend.UserInfo.Username = messageData.UserInfo.Username;
                    messageToSend.UserInfo.Message = messageData.UserInfo.Message;
                    break;
            }
            

            _logger.Debug($"Received message from: '{messageData.UserInfo.Username}'. Message: '{messageToSend.UserInfo.Message}'");

            Program.ConnectionManager.SendAll(messageToSend.ToByte());
        }

    }
}
