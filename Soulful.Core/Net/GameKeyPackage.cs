using LiteNetLib;
using LiteNetLib.Utils;
using Soulful.Core.Model;

namespace Soulful.Core.Net
{
    public struct GameKeyPackage
    {
        public GameKey Key { get; }
        public NetDataReader Data { get; }
        public NetPeer Player { get; }

        internal GameKeyPackage(GameKey key, NetDataReader reader, NetPeer player)
        {
            Key = key;
            Data = reader;
            Player = player;
        }
    }
}
