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
        DisconnectServerFull = 0,

        /// <summary>
        /// Disconnected as the max connections of the server has changed
        /// </summary>
        DisconnectLimitChanged = 1,

        /// <summary>
        /// Disconnected as the server is closing
        /// </summary>
        DisconnectServerClosed = 2,

        /// <summary>
        /// Disconnected as an invalid pin was provided
        /// </summary>
        DisconnectInvalidPin = 3,

        /// <summary>
        /// Disconnected due to an unknown error
        /// </summary>
        DisconnectUnknownError = 4,

        /// <summary>
        /// Disconnected due to a user action
        /// </summary>
        DisconnectUserAction = 5
    }
}
