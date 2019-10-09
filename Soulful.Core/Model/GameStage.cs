namespace Soulful.Core.Model
{
    public enum GameStage
    {
        /// <summary>
        /// The server is waiting for players to ready up
        /// </summary>
        AwaitingPlayerReady = 0,

        /// <summary>
        /// The server is sending card and czar data to clients
        /// </summary>
        SendingRoundData = 1,

        /// <summary>
        /// The server is waiting for clients to send their card selections
        /// </summary>
        AwaitingCardSelections = 2,

        /// <summary>
        /// The server is sending client card selections to the czar
        /// </summary>
        SendingCardsToCzar = 3,

        /// <summary>
        /// The server is waiting for the czar to make their pick
        /// </summary>
        AwaitingCzarPick = 4,

        /// <summary>
        /// The server is sending valid names to clients
        /// </summary>
        UpdatingLeaderboard = 5,

        /// <summary>
        /// The server needs to create an initial leaderboard and send it to all clients
        /// </summary>
        SendingInitialLeaderboard = 6
    }
}
