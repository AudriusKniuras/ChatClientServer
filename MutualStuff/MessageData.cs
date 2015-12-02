using System;
using System.Collections.Generic;
using System.Text;

namespace MutualStuff
{
    public class MessageData
    {
        public UserInformation UserInfo;

        public MessageData()
        {
            UserInfo = new UserInformation();
            UserInfo.MessageType = MessageType.Null;
        }

        public MessageData(byte[] data)
        {
            UserInfo = new UserInformation();

            // first 4 bytes show the message type
            UserInfo.MessageType = (MessageType) BitConverter.ToInt32(data, 4);

            //next 4 bytes are for the length of the name
            int nameLength = BitConverter.ToInt32(data, 8);

            // message length
            int messageLength = BitConverter.ToInt32(data, 12);

            UserInfo.Username = nameLength > 0 ? Encoding.ASCII.GetString(data, 16, nameLength) : null;

            UserInfo.Message = messageLength > 0 ? Encoding.ASCII.GetString(data, 16 + nameLength, messageLength) : null;

        }

        public byte[] ToByte()
        {
            List<byte> result = new List<byte>();

            result.AddRange(BitConverter.GetBytes((int)UserInfo.MessageType));

            result.AddRange(UserInfo.Username != null
                ? BitConverter.GetBytes(UserInfo.Username.Length)
                : BitConverter.GetBytes(0));

            result.AddRange(UserInfo.Message != null
                ? BitConverter.GetBytes(UserInfo.Message.Length)
                : BitConverter.GetBytes(0));

            if (UserInfo.Username != null)
                result.AddRange(Encoding.ASCII.GetBytes(UserInfo.Username));

            if (UserInfo.Message != null)
                result.AddRange(Encoding.ASCII.GetBytes(UserInfo.Message));

            result.InsertRange(0,BitConverter.GetBytes(result.Count+4));
            return result.ToArray();
        }


    }
}
