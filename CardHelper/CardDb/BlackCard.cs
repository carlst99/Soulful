using Realms;

namespace CardHelper.CardDb
{
    public class BlackCard : RealmObject, ICard
    {
        [PrimaryKey]
        public int Id { get; set; }

        public string Content { get; set; }

        /// <summary>
        /// Gets or sets the number of white cards that can be picked for this black card
        /// </summary>
        public int NumPicks { get; set; }
    }
}
