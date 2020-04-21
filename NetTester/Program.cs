using NetTester.Net;
using Serilog;
using Serilog.Events;
using System;
using System.Threading.Tasks;

namespace NetTester
{
    public static class Program
    {
        private static NetServer _server = new NetServer();
        private static NetClient _client = new NetClient();

        public static void Main()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console()
                .CreateLogger();

            int run = 0;
            do
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Run: " + run++);
                Console.ForegroundColor = ConsoleColor.White;
                Run();
            } while (Console.ReadKey().Key == ConsoleKey.Enter);
        }

        public static void Run()
        {
            _server.Start(1, "t");
            _client.Start("t", "name");

            _server.Stop();
            _client.SafeStop();
        }
    }

    /// <summary>
    /// Mimics the app class of Soulful.Core
    /// </summary>
    public static class App
    {
        /// <summary>
        /// Logs and creates an error
        /// </summary>
        /// <typeparam name="ExType">The type of error</typeparam>
        /// <param name="message">The error message</param>
        /// <returns>An exception</returns>
        public static ExType CreateError<ExType>(string message, LogEventLevel level = LogEventLevel.Error) where ExType : Exception, new()
        {
            try
            {
                ExType ex = (ExType)Activator.CreateInstance(typeof(ExType), message);
                Log.Write(level, ex, message);
                return ex;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error could not be created");
                return null;
            }
        }
    }
}
