using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace Encryption
{
    public class EncryptionAES
    {
        private readonly byte[] _salt = Encoding.ASCII.GetBytes("SomeRandomSalt");
        private readonly RijndaelManaged _rm;

        public EncryptionAES(string symmetricKey)
        {
            _rm = new RijndaelManaged();
            _rm.Mode = CipherMode.CBC;
            // trying to fix "padding is invalid and cannot be removed" exception
            //_rm.Padding = PaddingMode.Zeros;
            _rm.Key = (new Rfc2898DeriveBytes(symmetricKey, _salt, 5000)).GetBytes(_rm.KeySize/8);
        }

        public string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return String.Empty;

            string output = String.Empty;

            _rm.GenerateIV();
            
            using (ICryptoTransform encryptor = _rm.CreateEncryptor(_rm.Key, _rm.IV))
            using (var ms = new MemoryStream())
            {
                ms.Write(BitConverter.GetBytes(_rm.IV.Length),0, sizeof(int));
                ms.Write(_rm.IV,0,_rm.IV.Length);
                using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                {
                    using (var sw = new StreamWriter(cs))
                    {
                        sw.Write(plainText);
                    }
                }

                output = Convert.ToBase64String(ms.ToArray());
            }
            return output;
        }

        public string Decrypt(string cypherText)
        {
            if (string.IsNullOrEmpty(cypherText))
                return String.Empty;

            string plainText = String.Empty;

            byte[] messageBytes = Convert.FromBase64String(cypherText);

            using (var ms = new MemoryStream(messageBytes))
            {
                // get the inititalization vector from the encrypted stream
                _rm.IV = ReadByteArray(ms);
                ICryptoTransform decryptor = _rm.CreateDecryptor(_rm.Key, _rm.IV);

                using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                {
                    using (var sr = new StreamReader(cs))
                    {
                        plainText = sr.ReadToEnd();
                    }
                }
            }

            return plainText;
        }

        private static byte[] ReadByteArray(Stream s)
        {
            byte[] rawLength = new byte[sizeof(int)];
            if (s.Read(rawLength, 0, rawLength.Length) != rawLength.Length)
            {
                throw new SystemException("Stream did not contain properly formatted byte array");
            }
            byte[] buffer = new byte[BitConverter.ToInt32(rawLength,0)];

            if (s.Read(buffer, 0, buffer.Length) != buffer.Length)
            {
                throw new SystemException("Did not read byte array properly");
            }
            return buffer;

        }
    }
}
