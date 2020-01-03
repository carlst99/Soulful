using Realms;
using System.Collections.Generic;

namespace Soulful.Core.Model.CardDb
{
    public class Pack : RealmObject
    {
        [PrimaryKey]
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

        public override string ToString()
        {
            return Name;
        }
    }
}
