using LiteNetLib;
using System.Collections.Generic;

namespace Soulful.Core.Model
{
    public class Player
    {
        public NetPeer Peer { get; set; }
        public int Id => Peer.Id;
        public List<int> WhiteCards { get; set; }
        public int TotalScore { get; set; }
        public string Name { get; set; }

        public Player(NetPeer peer, string name)
        {
            Peer = peer;
            Name = name;
            WhiteCards = new List<int>();
            TotalScore = 0;
        }
    }
}
