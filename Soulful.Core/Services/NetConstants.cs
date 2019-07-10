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
        DisconnectServerFull = 0
    }
}
