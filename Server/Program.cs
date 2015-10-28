using System;
using System.Configuration;
using System.Net;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            String serverIp = ConfigurationManager.AppSettings["ServerIp"];
            String serverPort = ConfigurationManager.AppSettings["ServerPort"];

            Logger.Info($"Server will try to listen on {serverIp}:{serverPort}");

            EndPoint endPoint = new IPEndPoint(IPAddress.Parse(serverIp), UInt16.Parse(serverPort));

            using (ConnectionManager manager = new ConnectionManager(new ConnectionListener(endPoint), new MessageHandler()))
            {
                ConnectionManager = manager;
                Console.WriteLine("Press <Escape> to exit");
                while (Console.ReadKey().Key == ConsoleKey.Escape)
                { }

            }
        }

        private static void CurrentDomain_UnhandledException(Object sender, UnhandledExceptionEventArgs e)
        {
            Logger.Fatal(e.ExceptionObject);
            Console.WriteLine(e.ExceptionObject);
        }

        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public static ConnectionManager ConnectionManager { get; private set; }

    }
}
