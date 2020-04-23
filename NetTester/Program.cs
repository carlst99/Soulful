using NetTester.Net;
using Serilog;
using Serilog.Events;
using System;

namespace NetTester
{
    public static class Program
    {
        private readonly static NetServerService _server = new NetServerService();
        private readonly static NetClientService _client = new NetClientService();

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

        public async static void Run()
        {
            _server.Start(1, "t");
            await _client.Start("t", "name").ConfigureAwait(false);

            _server.Stop();
            _client.Stop();
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
