using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MutualStuff
{
    public class UserInformation
    {
        public string Username { get; set; }
        public string PublicKey { get; set; }
        public string Message { get; set; }
        public MessageType MessageType { get; set; }
    }
}
