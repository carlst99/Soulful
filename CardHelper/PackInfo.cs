namespace CardHelper
{
    public struct PackInfo
    {
        public string Key { get; set; }
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the point at which the black cards for this pack start
        /// </summary>
        public int BlackStartRange { get; set; }

        /// <summary>
        /// Gets or sets the number of black cards in the pack
        /// </summary>
        public int BlackCount { get; set; }

        /// <summary>
        /// Gets or sets the point at which the white cards for this pack start
        /// </summary>
        public int WhiteStartRange { get; set; }

        /// <summary>
        /// Gets or sets the number of white cards in the pack
        /// </summary>
        public int WhiteCount { get; set; }
    }
}
