using Realms;

namespace CardHelper.CardDb
{
    public class WhiteCard : RealmObject, ICard
    {
        [PrimaryKey]
        public int Id { get; set; }

        public string Content { get; set; }
    }
}
