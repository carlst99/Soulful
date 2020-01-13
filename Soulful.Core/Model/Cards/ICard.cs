namespace Soulful.Core.Model.Cards
{
    public interface ICard
    {
        /// <summary>
        /// Gets or sets the ID of this card, used in network communications and DB storage
        /// </summary>
        int Id { get; set; }

        /// <summary>
        /// Gets or sets the content of this card
        /// </summary>
        string Content { get; set; }
    }
}
