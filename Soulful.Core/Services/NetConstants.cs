using System;
using System.Collections.Generic;
using System.Text;

namespace Soulful.Core.Services
{
    public static class NetConstants
    {
        public const int PORT = 6259;

        public static byte[] GetKeyValue(NetKey value)
        {
            return new byte[] { (byte)value };
        }
    }

    public enum NetKey : byte
    {
        /// <summary>
        /// Disconnected as the server is full
        /// </summary>
        DisconnectServerFull = 0,

        /// <summary>
        /// Disconnected as the max connections of the server has changed
        /// </summary>
        DisconnectLimitChanged = 1,

        /// <summary>
        /// Disconnected as the server is closing
        /// </summary>
        DisconnectServerClosed = 2,

        /// <summary>
        /// Disconnected as an invalid pin was provided
        /// </summary>
        DisconnectInvalidPin = 3,

        /// <summary>
        /// Disconnected due to an unknown error
        /// </summary>
        DisconnectUnknownError = 4
    }
}
