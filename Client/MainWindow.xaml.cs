using System;
using System.Configuration;
using System.Net;
using System.Windows;
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
            MessageData messageData = new MessageData();
            messageData.UserInfo.Username = _username;
            messageData.UserInfo.MessageType = MessageType.Connect;;
            _connector.Send(messageData.ToByte());
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
            if(String.IsNullOrEmpty(MessageTextBox.Text))
            {
                MessageBox.Show("Please enter message text");
                return;
            }

            _connector.Send(MessageTextBox.Text);
        }
    }
}
