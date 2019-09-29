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
        GameStart = 0,

        /// <summary>
        /// Indicates to the client that they have successfully joined the game server
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
        /// The client is sending their favourite pick and exiting czar mode
        /// </summary>
        CzarPick = 7,

        /// <summary>
        /// The client is sending their selected white cards
        /// </summary>
        ClientSendWhiteCards = 8,

        /// <summary>
        /// Indicates to the server that the client is ready to start
        /// </summary>
        ClientReady = 9
    }
}
