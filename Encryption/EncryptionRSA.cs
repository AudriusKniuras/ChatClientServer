using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Security.Cryptography;
using System.Text;

namespace Encryption
{
    public class EncryptionRSA
    {
        private static int _symmetricKeySize = 16;
        public BigInteger D; // public private key??
        public BigInteger N { get; private set; }
        public BigInteger E { get; private set; }
        private BigInteger _fi;

        public EncryptionRSA(bool generateKeys)
        {
            if (generateKeys)
                GenerateKeys();
        }

        public EncryptionRSA()
        {}

        private void GenerateKeys()
        {
            Random rand = new Random();
            N = 0;
            _fi = 0;
            

            while (!testVariable(N,128) || !testVariable(_fi,128))
            {
                BigInteger prime1 = BigInteger.genPseudoPrime(64, 5, rand);
                BigInteger prime2 = BigInteger.genPseudoPrime(64, 5, rand);
                N = prime1*prime2;

                _fi = (prime1 - 1)*(prime2 - 1);
            }

            var ed = coprimeAndInverse(_fi);
            //coprime
            E = ed.Item1;
            //inverse
            D = ed.Item2; //trapdoor

        }

        private bool testVariable(BigInteger number, int bitLength)
        {
            int bits = 0;
            do
            {
                bits++;
                number /= 2;
            } while (number != 0);
            if (bits == bitLength)
                return true;

            return false;
        }

        public BigInteger getRandom(int length)
        {
            Random random = new Random();
            byte[] data = new byte[length];
            random.NextBytes(data);
            return new BigInteger(data);
        }

        //TODO: perdaryti
        // gcd Extended Euclidian
        /* OLD CoprimeAndInverse
    private Tuple<BigInteger, BigInteger> coprimeAndInverse(BigInteger fi, Random rand)
    {
        bool isCoprime = false;
        BigInteger e = 0, selectedE = 0;

        // Bezout Coefficients
        BigInteger s = 1, t = 0;
        //Stack<BigInteger> q = new Stack<BigInteger>();
        List<BigInteger> q = new List<BigInteger>();
        //if trapdoor is negative, do it again
        while (t < 100)
        {
            while (isCoprime == false)
            {
                e = rand.Next(10000, Int32.MaxValue);
                selectedE = e;
                BigInteger gcd = 0;
                q.Clear();

                while (e > 0)
                {
                    if (e > 0)
                    {
                        //q.Push(fi/e);
                        //fi = q.Peek() * e + (fi % e);
                        q.Add(fi/e);
                        fi = q[q.Count - 1]*e + (fi%e);

                        gcd = e;
                    }
                    BigInteger temp = e;
                    e = fi%e;
                    fi = temp;
                }
                if (gcd == 1)
                    isCoprime = true;
            }

            // inverse
            foreach (BigInteger number in q)
            {
                BigInteger tempS = s;
                s = t;
                t = tempS - (number*s);
            }
        }
        return new Tuple<BigInteger, BigInteger>(selectedE,t);
    }
    */

        //perdarytas
        private Tuple<BigInteger, BigInteger> coprimeAndInverse(BigInteger fi)
        {
            int i = 0;
            BigInteger[] eArray = {3, 5, 17, 257, 65537};
            BigInteger old_e = 0;

            List<BigInteger> quotients = new List<BigInteger>();
            bool isCoprime = false;

            while (isCoprime == false)
            {
                BigInteger e = eArray[i++];
                old_e = e;
                while (e != 0)
                {
                    BigInteger quotient = fi/e;
                    BigInteger temp = e;
                    e = fi - quotient*e;
                    fi = temp;
                    quotients.Add(quotient);
                }
                if (fi == 1)
                {
                    isCoprime = true;
                }
            }

            int j = 0;
            BigInteger s = 1, t = 0;
            quotients.Reverse();
            while (j < quotients.Count)
            {
                var oldT = t;
                var oldS = s;

                s = oldT;
                t = oldS - quotients[j++]*oldT;
            }
            if (t < 0) //neisitikines, kad sitaip
                t *= -1;
            return new Tuple<BigInteger, BigInteger>(old_e, t);
        }

        //symmetric key generation
        public string CreateSymmetricKey()
        {
            byte[] key = new byte[_symmetricKeySize];
            var rng = new RNGCryptoServiceProvider();
            rng.GetBytes(key);

            return Convert.ToBase64String(key);

        }

        public string EncryptSymmetricKey(string symmKey, BigInteger e, BigInteger n)
        {
            BigInteger cryptoSymmKey = new BigInteger(Convert.FromBase64String(symmKey));

            cryptoSymmKey = cryptoSymmKey.modPow(e,n);

            string symmKeyString = Convert.ToBase64String(cryptoSymmKey.getBytes());

            return symmKeyString;
        }

        public string DecryptSymmetricKey(string encrSymmKey, BigInteger d, BigInteger n)
        {
            BigInteger encryptedSymmKey = new BigInteger(Convert.FromBase64String(encrSymmKey));

            //can be redone with RSA for efficiency
            BigInteger decryptedSymmKey = encryptedSymmKey.modPow(d, n);

            byte[] symmKeyBytes = decryptedSymmKey.getBytes();

            string symmKey = Encoding.ASCII.GetString(symmKeyBytes);

            return symmKey;
        }



        long LongRandom(long min, long max, Random rand)
        {
            byte[] buf = new byte[8];
            rand.NextBytes(buf);
            long longRand = BitConverter.ToInt64(buf, 0);
            return (Math.Abs(longRand % (max - min)) + min);
        }
    }
} 
