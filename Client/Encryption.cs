using Encryption;

namespace Client
{
    class Encryption
    {
        public BigInteger D { get; } //public..?
        public BigInteger E { get; }
        public BigInteger N { get; }
        public string SymmKey { get; set; }

        public BigInteger serverE { get; set; }
        public BigInteger serverN { get; set; }

        public Encryption(BigInteger d, BigInteger e, BigInteger n)
        {
            D = d;
            E = e;
            N = n;
        }

        public string decryptMessage(string message)
        {

            if (SymmKey != null)
            {
                EncryptionAES aes = new EncryptionAES(SymmKey);
                return aes.Decrypt(message);
            }
            return string.Empty;
        }
    }
}
