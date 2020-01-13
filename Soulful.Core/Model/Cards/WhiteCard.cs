namespace Soulful.Core.Model.Cards
{
    public class WhiteCard : ICard
    {
        public int Id { get; set; }
        public string Content { get; set; }

        public WhiteCard()
        {
        }

        public WhiteCard(int id, string content)
        {
            Id = id;
            Content = content;
        }

        public override string ToString() => Content;
    }
}
