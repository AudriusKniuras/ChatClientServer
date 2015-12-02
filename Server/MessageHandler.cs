using System;
using System.Diagnostics.SymbolStore;
using System.Text;
using MutualStuff;
using Encryption;
namespace Server
{
    internal class MessageHandler
    {
        private readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        EncryptionRSA _serverKeys = new EncryptionRSA(true);

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
                    if (connection.ConnectionInformation == null)
                    {
                        connection.ConnectionInformation = new ConnectionInformation(messageToSend.UserInfo.Username,
                            connection.Id);
                        Program.ConnectionManager.UsersList.Add(connection.ConnectionInformation);
                    }
                    else
                    {
                        connection.ConnectionInformation.ChangeUsername(messageData.UserInfo.Username);
                    }
                    messageToSend.UserInfo.Message = ">>> " + messageData.UserInfo.Username + " connected";
                    Program.ConnectionManager.SendAll(messageToSend.ToByte());
                    break;

                case MessageType.Disconnect:
                    messageToSend.UserInfo.MessageType = MessageType.Disconnect;
                    foreach (var connections in Program.ConnectionManager.UsersList)
                    {
                        if (connections.Id == connection.Id)
                        {
                            Program.ConnectionManager.UsersList.Remove(connections);
                            break;
                        }
                    }
                    messageToSend.UserInfo.Message = "<<< " + messageData.UserInfo.Username + " disconnected";
                    Program.ConnectionManager.SendAll(messageToSend.ToByte());
                    break;

                case MessageType.Message:
                    messageToSend.UserInfo.Username = messageData.UserInfo.Username;
                    messageToSend.UserInfo.Message = messageData.UserInfo.Message;
                    Program.ConnectionManager.SendAll(messageToSend.ToByte());
                    break;

                case MessageType.ParameterE:
                    connection.ConnectionInformation.ChangeE(new BigInteger(messageData.UserInfo.Message, 10));
                    messageToSend.UserInfo.Username = "server";
                    messageToSend.UserInfo.MessageType = MessageType.ParameterE;
                    messageToSend.UserInfo.Message = _serverKeys.E.ToString(10);
                    Program.ConnectionManager.SendAll(messageToSend.ToByte());
                    break;


                case MessageType.ParameterN:
                    connection.ConnectionInformation.ChangeN(new BigInteger(messageData.UserInfo.Message, 10));
                    messageToSend.UserInfo.Username = "server";
                    messageToSend.UserInfo.MessageType = MessageType.ParameterN;
                    messageToSend.UserInfo.Message = _serverKeys.N.ToString(10);
                    Program.ConnectionManager.SendAll(messageToSend.ToByte());
                    break;

                case MessageType.SymmetricKey:
                    foreach (var user in Program.ConnectionManager.UsersList)
                    {
                        if (user.Username == messageData.UserInfo.Username)
                        {
                            user.symmKey = messageData.UserInfo.Message; //encrypted
                            break;
                        }
                    }
                    break;

                case MessageType.CryptoMessage:
                    // "name,message"
                    string[] receiverUsernameAndMessage = messageData.UserInfo.Message.Split(new char[] {','}, 2);
                    string encryptedMessage = encryptMessage(receiverUsernameAndMessage[1],
                        receiverUsernameAndMessage[0], messageData.UserInfo.Username);

                    //encrypted symm key, receiver, encrypted message
                    messageToSend.UserInfo.Username = messageData.UserInfo.Username;
                    messageToSend.UserInfo.MessageType = MessageType.CryptoMessage;
                    messageToSend.UserInfo.Message = symmKeyBytes + "," + receiverUsernameAndMessage[0] + "," + encryptedMessage;
                    Program.ConnectionManager.SendAll(messageToSend.ToByte());
                    break;
            }
            

            _logger.Debug($"Received message from: '{messageData.UserInfo.Username}'. Message: '{messageData.UserInfo.Message}'. Message Type: " +
                          $"{messageData.UserInfo.MessageType}");
            //Program.ConnectionManager.messageQueue.RemoveAt(0);
            
        }

        private string symmKeyBytes; //encrypted with RSA with receiver's public key BASE64
        private string encryptMessage(string message, string receiverUsername, string senderUsername)
        {
            BigInteger n = 0, e = 0;
            string encryptedSymmKey = "";
            foreach (var user in Program.ConnectionManager.UsersList)
            {
                if (user.Username == receiverUsername)
                {
                    n = user.N;
                    e = user.E;
                    break;
                }
                if (user.Username == senderUsername)
                {
                    encryptedSymmKey = user.symmKey;
                }
            }

            string decryptedSymmKey = _serverKeys.DecryptSymmetricKey(encryptedSymmKey, _serverKeys.D, _serverKeys.N);
            symmKeyBytes            = _serverKeys.EncryptSymmetricKey(decryptedSymmKey, e, n);



            var EncryptionAES = new EncryptionAES(decryptedSymmKey);

            return EncryptionAES.Encrypt(message);
        }
    }
}
