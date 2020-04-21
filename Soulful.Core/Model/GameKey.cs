namespace Soulful.Core.Model
{
    /// <summary>
    /// Contains gameplay-related keys
    /// </summary>
    public enum GameKey
    {
        /// <summary>
        /// The server has started the game
        /// </summary>
        GameStart,

        /// <summary>
        /// Indicates to the client that they have successfully joined the game server
        /// </summary>
        JoinedGame,

        /// <summary>
        /// The server has stopped the game
        /// </summary>
        GameStop,

        /// <summary>
        /// The server is sending white card numbers
        /// </summary>
        SendWhiteCards,

        /// <summary>
        /// The server is sending a black card
        /// </summary>
        SendBlackCard,

        /// <summary>
        /// Inidicates that this client should switch to czar mode
        /// </summary>
        InitiateCzar,

        /// <summary>
        /// The server is sending white cards to add to the czar pick list
        /// </summary>
        SendCzarWhiteCards,

        /// <summary>
        /// The client is sending their favourite pick and exiting czar mode
        /// </summary>
        CzarPick,

        /// <summary>
        /// The client is sending their selected white cards
        /// </summary>
        ClientSendWhiteCards,

        /// <summary>
        /// Indicates to the server that the client is ready to start
        /// </summary>
        ClientReady,

        /// <summary>
        /// The server is sending an updated leaderboard delta package
        /// </summary>
        UpdatingLeaderboard,

        /// <summary>
        /// The server is sending the initial leaderboard, complete with names
        /// </summary>
        SendingInitialLeaderboard
    }
}
