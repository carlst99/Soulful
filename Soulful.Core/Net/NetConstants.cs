using LiteNetLib.Utils;

namespace Soulful.Core.Net
{
    public static class NetConstants
    {
        public const int PORT = 6259;

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
