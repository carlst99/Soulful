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
        DisconnectServerClosed = 2
    }
}
