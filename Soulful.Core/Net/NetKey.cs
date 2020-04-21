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
        ServerFull,

        /// <summary>
        /// Disconnected as the max connections of the server has changed
        /// </summary>
        ServerLimitChanged,

        /// <summary>
        /// Disconnected as the server is closing
        /// </summary>
        ServerClosed,

        /// <summary>
        /// Disconnected as an invalid pin was provided
        /// </summary>
        InvalidPin,

        /// <summary>
        /// Disconnected due to an unknown error
        /// </summary>
        DisconnectUnknownError,

        /// <summary>
        /// Kicked by server
        /// </summary>
        Kicked
    }
}
