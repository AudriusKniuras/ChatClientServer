using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Encryption;

namespace Server
{
    class ConnectionInformation
    {
        public string Username { get; private set; }
        public Guid Id { get; }
        public BigInteger N { get; private set; }
        public BigInteger E { get; private set; }
        public string symmKey { get; set; }

        public ConnectionInformation(string username, Guid id)
        {
            Username = username;
            Id = id;
        }

        public void ChangeN(BigInteger n)
        {
            N = n;
        }

        public void ChangeE(BigInteger e)
        {
            E = e;
        }

        public void ChangeUsername(string username)
        {
            Username = username;
        }

    }
}
