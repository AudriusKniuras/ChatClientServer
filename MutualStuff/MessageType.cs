namespace MutualStuff
{
    public enum MessageType
    {
        Connect,        // client connects to server
        Disconnect,
        List,           // user list sent to clients
        Message,        // simple message
        CryptoMessage,
        ParameterList,  // parameters for encryption
        Null,           // unexpected message
    }
}
