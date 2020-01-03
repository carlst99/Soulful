using IntraMessaging;
using MvvmCross;
using MvvmCross.IoC;
using MvvmCross.ViewModels;
using Plugin.DeviceInfo;
using Plugin.DeviceInfo.Abstractions;
using Serilog;
using Serilog.Events;
using Soulful.Core.Net;
using Soulful.Core.ViewModels;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("MvvmCrossCoreTestProject")]

namespace Soulful.Core
{
    public class App : MvxApplication
    {
        public const string LOG_FILE_NAME = "log.log";

        public override void Initialize()
        {
            CreatableTypes()
                .EndingWith("Service")
                .AsInterfaces()
                .RegisterAsLazySingleton();

            RegisterAppStart<HomeViewModel>();

            Mvx.IoCProvider.RegisterSingleton<IIntraMessenger>(IntraMessenger.Instance);

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Debug()
                .WriteTo.File(GetAppdataFilePath(LOG_FILE_NAME))
                .CreateLogger();

            if (CrossDeviceInfo.IsSupported)
            {
                Log.Information("Started on {model} running {platform} {version}",
                    CrossDeviceInfo.Current.Model,
                    CrossDeviceInfo.Current.Platform,
                    CrossDeviceInfo.Current.Version);
            }
        }

        #region Error Helpers

        /// <summary>
        /// Logs and creates an error
        /// </summary>
        /// <typeparam name="ExType">The type of error</typeparam>
        /// <param name="message">The error message</param>
        /// <returns>An exception</returns>
        public static ExType CreateError<ExType>(string message, bool log = true, LogEventLevel level = LogEventLevel.Error) where ExType : Exception, new()
        {
            try
            {
                ExType ex = (ExType)Activator.CreateInstance(typeof(ExType), message);
                if (log)
                    Log.Write(level, ex, message);
                return ex;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error could not be created");
                return null;
            }
        }

        /// <summary>
        /// Logs an error and returns it
        /// </summary>
        /// <typeparam name="ExType">The type of exception to create and return</typeparam>
        /// <param name="message">The error message</param>
        /// <param name="exception">The inner exception to log</param>
        /// <returns>An exception</returns>
        public static Exception LogError(string message, Exception exception, bool log = true)
        {
            if (log)
                Log.Error(exception, message);
            return exception;
        }

        #endregion

        #region Appdata Helpers

        /// <summary>
        /// Gets the path to the appdata store of respective platforms
        /// </summary>
        /// <returns>
        /// <see cref="Environment.SpecialFolder.Personal"/> when running on Android
        /// <see cref="Environment.SpecialFolder.MyDocuments"/> when running on iOS
        /// <see cref="Environment.SpecialFolder.ApplicationData"/> when running on any other platform
        /// </returns>
        public static string GetPlatformAppdataPath()
        {
            string path;

            if (CrossDeviceInfo.IsSupported)
            {
                path = CrossDeviceInfo.Current.Platform switch
                {
                    Platform.Android => Environment.GetFolderPath(Environment.SpecialFolder.Personal),
                    Platform.iOS => Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    _ => Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                };
            } else
            {
                path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            }

            path = Path.Combine(path, "Soulful");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            return path;
        }

        /// <summary>
        /// Gets the path to a file in the local appdata
        /// </summary>
        /// <param name="fileName">The name of the file to resolve the path to</param>
        /// <returns></returns>
        public static string GetAppdataFilePath(string fileName) => Path.Combine(GetPlatformAppdataPath(), fileName);

        #endregion
    }
}
