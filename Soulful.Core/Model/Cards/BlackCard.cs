namespace Soulful.Core.Model.Cards
{
    public class BlackCard : ICard
    {
        public int Id { get; set; }
        public string Content { get; set; }

        /// <summary>
        /// Gets or sets the number of white cards that can be picked for this black card
        /// </summary>
        public int NumPicks { get; set; }

        public override string ToString() => Content;
    }
}
