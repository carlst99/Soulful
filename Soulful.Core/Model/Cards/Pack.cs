using System.Collections.Generic;

namespace Soulful.Core.Model.Cards
{
    public class Pack
    {
        #region Properties

        /// <summary>
        /// Gets or sets the ID of this pack
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the name of this pack
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the white cards contained in this pack
        /// </summary>
        public IList<WhiteCard> WhiteCards { get; }

        /// <summary>
        /// Gets or sets the black cards contained in this pack
        /// </summary>
        public IList<BlackCard> BlackCards { get; }

        #endregion

        #region Ctors

        public Pack()
        {
        }

        public Pack(int id, string name)
        {
            Id = id;
            Name = name;
            WhiteCards = new List<WhiteCard>();
            BlackCards = new List<BlackCard>();
        }

        public Pack(int id, string name, IList<BlackCard> blackCards, IList<WhiteCard> whiteCards)
        {
            Id = id;
            Name = name;
            BlackCards = blackCards;
            WhiteCards = whiteCards;
        }

        #endregion

        public override string ToString() => Name;
    }
}
