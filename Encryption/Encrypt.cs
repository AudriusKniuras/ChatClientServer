using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Encryption
{
    public class Encrypt
    {
        private uint _d, _e;
        public uint PublicKey { get; private set; }
        private uint _fi; // private key

        private void GenerateKeys()
        {
            string[] primes = System.IO.File.ReadAllLines(Environment.CurrentDirectory + @"\primes.txt");
            Random rand = new Random();

            uint prime1 = uint.Parse(primes[rand.Next(0, primes.Length)]);
            uint prime2 = uint.Parse(primes[rand.Next(0, primes.Length)]);
            PublicKey = prime1*prime2;

            _fi = (prime1 - 1)*(prime2 - 1);

            var ed = coprimeAndInverse(_fi, rand);
            //coprime
            _e = ed.Item1;
            //inverse
            _d = ed.Item2;

        }
        // gcd Extended Euclidian
        private Tuple<uint,uint> coprimeAndInverse(uint fi, Random rand)
        {
            bool isCoprime = false;
            uint e = 0;

            // Bezout Coefficients
            uint s = 1, t = 0;
            Stack<uint> q = new Stack<uint>();

            while (isCoprime == false)
            {
                e = (uint)rand.Next(10000, (int)(fi - 1));
                uint gcd = 0;
                q.Clear();

                while (e > 0)
                {
                    if (e > 0)
                    {
                        q.Push(fi/e);
                        fi = q.Peek()*e + (fi%e);
                        gcd = e;
                    }
                    uint temp = e;
                    e = fi%e;
                    fi = temp;
                }
                if (gcd == 1)
                    isCoprime = true;
            }
            foreach (uint number in q)
            {
                uint tempS = s;
                s = t;
                t = tempS - (number*s);
            }
            return new Tuple<uint, uint>(e,t);
        }

        public byte[] EncryptRSA(string message)
        {
            byte[] messageBytes = Encoding.Unicode.GetBytes(message);
            byte[] encryptedMessage;
            RSAParameters rsaParameters = new RSAParameters();

            rsaParameters.Exponent = Encoding.Unicode.GetBytes(_e.ToString());
            rsaParameters.Modulus = Encoding.Unicode.GetBytes(_fi.ToString());

            using (RSACryptoServiceProvider RSAcsp = new RSACryptoServiceProvider())
            {
                RSAcsp.ImportParameters(rsaParameters);
                encryptedMessage = RSAcsp.Encrypt(messageBytes, false);
            }

            return encryptedMessage;
        }

        public void DecryptRSA()
        {
            
        }
    }
}
