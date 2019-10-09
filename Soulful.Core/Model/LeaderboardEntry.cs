namespace Soulful.Core.Model
{
    public class LeaderboardEntry
    {
        /// <summary>
        /// Gets the network ID of the player
        /// </summary>
        public int PlayerId { get; }

        /// <summary>
        /// Gets the name of the player
        /// </summary>
        public string PlayerName { get; }

        /// <summary>
        /// Gets or sets the score of the player
        /// </summary>
        public int Score { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether or not this player is top of the leaderboard
        /// </summary>
        public bool IsTop { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether or not this player is bottom of the leaderboard
        /// </summary>
        public bool IsBottom { get; set; }

        public LeaderboardEntry(int playerId, string playerName, int score = 0)
        {
            PlayerId = playerId;
            PlayerName = playerName;
            Score = score;
        }

        /// <summary>
        /// Resets the <see cref="IsTop"/> and <see cref="IsBottom"/> properties
        /// </summary>
        public void Reset()
        {
            IsTop = false;
            IsBottom = false;
        }
    }
}
