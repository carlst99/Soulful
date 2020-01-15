using LiteNetLib;
using Soulful.Core.Model.Cards;
using System.Collections.Generic;

namespace Soulful.Core.Model
{
    public class Player
    {
        private int _score;

        #region Properties

        /// <summary>
        /// Gets or sets the network peer of this player
        /// </summary>
        public readonly NetPeer Peer;

        /// <summary>
        /// Gets the peer ID of this player
        /// </summary>
        public int Id => Peer.Id;

        /// <summary>
        /// Gets or sets the list of white cards that this player currently holds
        /// </summary>
        public List<WhiteCard> WhiteCards { get; set; }

        /// <summary>
        /// Gets or sets the list of white cards that this player has selected in the last round
        /// </summary>
        public List<WhiteCard> SelectedWhiteCards { get; set; }

        /// <summary>
        /// Gets or sets the total score for this player
        /// </summary>
        public int Score
        {
            get => _score;
            set
            {
                _score = value;
                NeedsLeaderboardUpdate = true;
            }
        }

        /// <summary>
        /// Gets or sets the players name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether or not this player is ready to participate in the game
        /// </summary>
        public bool IsReady { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether or not this player is the czar
        /// </summary>
        public bool IsCzar { get; set; }

        /// <summary>
        /// Gets or sets a value which indicates whether this player's leaderboard entry needs an update
        /// </summary>
        public bool NeedsLeaderboardUpdate { get; set; }

        #endregion

        public Player(NetPeer peer, string name)
        {
            Peer = peer;
            Name = name;
            WhiteCards = new List<WhiteCard>();
            SelectedWhiteCards = new List<WhiteCard>();
            Score = 0;
            NeedsLeaderboardUpdate = true;
            IsReady = false;
            IsCzar = false;
        }
    }
}
