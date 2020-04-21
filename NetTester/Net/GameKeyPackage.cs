using LiteNetLib;
using LiteNetLib.Utils;

namespace NetTester.Net
{
    public struct GameKeyPackage
    {
        public GameKey Key { get; }
        public NetDataReader Data { get; }
        public NetPeer Peer { get; }

        internal GameKeyPackage(GameKey key, NetDataReader reader, NetPeer player)
        {
            Key = key;
            Data = reader;
            Peer = player;
        }

        public override bool Equals(object obj)
        {
            return obj is GameKeyPackage p
                && p.Key.Equals(Key)
                && p.Peer.Equals(Peer)
                && p.Data.Equals(Data);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = (hash * 13) + Key.GetHashCode();
                hash = (hash * 13) + Peer.GetHashCode();
                return (hash * 13) + Data.GetHashCode();
            }
        }

        public static bool operator ==(GameKeyPackage left, GameKeyPackage right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(GameKeyPackage left, GameKeyPackage right)
        {
            return !(left == right);
        }
    }
}
