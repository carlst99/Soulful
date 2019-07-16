namespace Soulful.Core.Net
{
    /// <summary>
    /// Contains gameplay-related keys
    /// </summary>
    public enum GameKey
    {
        /// <summary>
        /// The server has started the game
        /// </summary>
        GameStart = 0,

        /// <summary>
        /// The client has successfully joined the game server
        /// </summary>
        JoinedGame = 1,

        /// <summary>
        /// The server has stopped the game
        /// </summary>
        GameStop = 2,

        /// <summary>
        /// The server is sending white card numbers
        /// </summary>
        SendWhiteCards = 3,

        /// <summary>
        /// The server is sending a black card
        /// </summary>
        SendBlackCard = 4,

        /// <summary>
        /// Inidicates that this client should switch to czar mode
        /// </summary>
        InitiateCzar = 5,

        /// <summary>
        /// The server is sending white cards to add to the czar pick list
        /// </summary>
        SendCzarWhiteCards = 6,

        /// <summary>
        /// Inidicates the czar client should reply with their pick, and may then leave czar mode
        /// </summary>
        CzarChooseNow = 7
    }
}
