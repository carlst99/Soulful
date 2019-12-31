using Realms;

namespace Soulful.Core.Model.CardDb
{
    public interface ICard
    {
        [PrimaryKey]
        int Id { get; set; }

        /// <summary>
        /// Gets or sets the content of this card
        /// </summary>
        string Content { get; set; }
    }
}
