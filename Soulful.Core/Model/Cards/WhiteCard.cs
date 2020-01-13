namespace Soulful.Core.Model.Cards
{
    public class WhiteCard : ICard
    {
        public int Id { get; set; }
        public string Content { get; set; }

        public override string ToString() => Content;
    }
}
