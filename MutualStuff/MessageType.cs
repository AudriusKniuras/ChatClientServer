namespace MutualStuff
{
    public enum MessageType
    {
        Connect,        // client connects to server
        Disconnect,
        List,           // user list sent to clients
        Message,        // simple message
        CryptoMessage,
        ParameterN,
        ParameterE,     // parameters for encryption
        SymmetricKey,
        Null,           // unexpected message
    }
}
