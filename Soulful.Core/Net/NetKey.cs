namespace Soulful.Core.Net
{
    /// <summary>
    /// Contains network-related keys
    /// </summary>
    public enum NetKey : byte
    {
        /// <summary>
        /// Disconnected as the server is full
        /// </summary>
        ServerFull = 0,

        /// <summary>
        /// Disconnected as the max connections of the server has changed
        /// </summary>
        ServerLimitChanged = 1,

        /// <summary>
        /// Disconnected as the server is closing
        /// </summary>
        ServerClosed = 2,

        /// <summary>
        /// Disconnected as an invalid pin was provided
        /// </summary>
        InvalidPin = 3,

        /// <summary>
        /// Disconnected due to an unknown error
        /// </summary>
        DisconnectUnknownError = 4,

        /// <summary>
        /// Kicked by server
        /// </summary>
        Kicked = 6
    }
}
