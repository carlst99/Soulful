﻿using LiteNetLib;
using LiteNetLib.Utils;

namespace Soulful.Core.Model
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
    }
}
