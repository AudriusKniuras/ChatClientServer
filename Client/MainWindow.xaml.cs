using System;
using System.Configuration;
using System.Net;
using System.Text;
using System.Windows;
using Encryption;
using MutualStuff;

namespace Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(string username)
        {        
            InitializeComponent();
            _username = username;
        }

        private Encryption encryption;
        private EncryptionRSA encryptionRsa;
        private readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
        private ConnectionConnector _connector;
        private readonly string _username;

        private void Window_Loaded(Object sender, RoutedEventArgs e)
        {
            
            //Catch all exceptions
            Application.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
            Application.Current.Exit += Current_Exit;

            String serverIp = ConfigurationManager.AppSettings["ServerIp"];
            String serverPort = ConfigurationManager.AppSettings["ServerPort"];

            _logger.Info($"Client will try to connect to {serverIp}:{serverPort}");

            _connector = new ConnectionConnector(new IPEndPoint(IPAddress.Parse(serverIp), UInt16.Parse(serverPort)), _username);
            MessageData messageData = new MessageData
            {
                UserInfo =
                {
                    Username = _username,
                    MessageType = MessageType.Connect
                }
            };
            _connector.Send(messageData.ToByte());

            bool succeeded = false;
            while (succeeded == false)
            {
                try
                {
                    encryptionRsa = new EncryptionRSA(true);
                    encryption = new Encryption(encryptionRsa.D, encryptionRsa.E,encryptionRsa.N);
                    messageData.UserInfo.MessageType = MessageType.ParameterN;
                    messageData.UserInfo.Message = encryption.N.ToString(10);
                    _connector.Send(messageData.ToByte());
                    

                    //kartais N neissiuncia kazkodel
                    messageData.UserInfo.MessageType = MessageType.ParameterE;
                    messageData.UserInfo.Message = encryption.E.ToString(10);
                    _connector.Send(messageData.ToByte());

                    succeeded = true;
                    
                }
                catch (Exception)
                {
                }
            }

            _connector.OnMessageReceived += OnMessageReceivedEventHandler;
        }

        private void OnMessageReceivedEventHandler(MessageData messageData)
        {
            Dispatcher.Invoke(() =>
                {
                    _logger.Debug($"Received message: '{messageData.UserInfo.Message}' from '{messageData.UserInfo.Username}'" +
                                  $" Message type: '{messageData.UserInfo.MessageType}'");
                    if (messageData.UserInfo.MessageType == MessageType.Connect ||
                        messageData.UserInfo.MessageType == MessageType.Disconnect)
                    {
                        ChatTextBox.AppendText($"{messageData.UserInfo.Message}{Environment.NewLine}");
                    }

                    else if (messageData.UserInfo.MessageType == MessageType.ParameterE)
                    {
                        encryption.serverE = new BigInteger(messageData.UserInfo.Message,10);
                    }
                    else if (messageData.UserInfo.MessageType == MessageType.ParameterN)
                    {
                        encryption.serverN = new BigInteger(messageData.UserInfo.Message,10);
                    }


                    else if (messageData.UserInfo.MessageType == MessageType.CryptoMessage)
                    {
                        // encrypted symm key, receiver, encrypted message
                        string[] receiverUsernameAndMessage = messageData.UserInfo.Message.Split(new char[] { ',' }, 3);
                        if (_username == receiverUsernameAndMessage[1])
                        {
                            string decryptedSymmKey = encryptionRsa.DecryptSymmetricKey(receiverUsernameAndMessage[0],
                                encryption.D, encryption.N);
                            EncryptionAES aes = new EncryptionAES(decryptedSymmKey);
                            string message = aes.Decrypt(receiverUsernameAndMessage[2]);
                            message.TrimEnd();
                            ChatTextBox.AppendText($"{messageData.UserInfo.Username}: (private) {message} {Environment.NewLine}");
                        }
                        else
                        {
                            ChatTextBox.AppendText($"{messageData.UserInfo.Username}: {receiverUsernameAndMessage[2]}" +
                                                   $"{Environment.NewLine}");
                        }
                    }
                    else
                    {
                        ChatTextBox.AppendText(
                            $"{messageData.UserInfo.Username}: {messageData.UserInfo.Message}{Environment.NewLine}");
                    }
                });
        }

        private void Current_Exit(Object sender, ExitEventArgs e)
        {
            _connector?.Dispose();
        }

        private void Current_DispatcherUnhandledException(Object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            _logger.Fatal(e.Exception);
        }

        private void SendButton_Click(Object sender, RoutedEventArgs e)
        {
            if(string.IsNullOrEmpty(MessageTextBox.Text))
            {
                MessageBox.Show("Please enter message text");
                return;
            }
            // encrypted message:
            // Encrypted:"username","message"
            if (MessageTextBox.Text.StartsWith("Encrypted:"))
            {
                encryption.SymmKey = encryptionRsa.CreateSymmetricKey();
                string encryptedKey = encryptionRsa.EncryptSymmetricKey(encryption.SymmKey, encryption.serverE,
                    encryption.serverN);
                _connector.Send(encryptedKey, MessageType.SymmetricKey);

                _connector.Send(MessageTextBox.Text.Substring(10), MessageType.CryptoMessage);
            }
            else
            {
                _connector.Send(MessageTextBox.Text, MessageType.Message);
            }
            
        }
    }
}
