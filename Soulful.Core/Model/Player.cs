using LiteNetLib;
using System.Collections.Generic;

namespace Soulful.Core.Model
{
    public class Player
    {
        /// <summary>
        /// Gets or sets the network peer of this player
        /// </summary>
        public NetPeer Peer { get; set; }

        /// <summary>
        /// Gets the peer ID of this player
        /// </summary>
        public int Id => Peer.Id;

        /// <summary>
        /// Gets or sets the list of white cards that this player currently holds
        /// </summary>
        public List<int> WhiteCards { get; set; }

        /// <summary>
        /// Gets or sets the list of white cards that this player has selected in the last round
        /// </summary>
        public List<int> SelectedWhiteCards { get; set; }

        /// <summary>
        /// Gets or sets the total score for this player
        /// </summary>
        public int TotalScore { get; set; }

        /// <summary>
        /// Gets or sets the players name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether or not this player is ready to participate in the game
        /// </summary>
        public bool IsReady { get; set; }

        public Player(NetPeer peer, string name)
        {
            Peer = peer;
            Name = name;
            WhiteCards = new List<int>();
            SelectedWhiteCards = new List<int>();
            TotalScore = 0;
            IsReady = false;
        }
    }
}
