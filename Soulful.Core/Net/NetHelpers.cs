using LiteNetLib.Utils;

namespace Soulful.Core.Net
{
    public static class NetHelpers
    {
        public const int POLL_DELAY = 15;

        public static byte[] GetKeyValue(NetKey value) => GetKeyValue((byte)value);

        public static NetDataWriter GetKeyValue(GameKey value)
        {
            NetDataWriter writer = new NetDataWriter();
            writer.Put((byte)value);
            return writer;
        }

        private static byte[] GetKeyValue(byte value) => new byte[] { value };
    }
}
