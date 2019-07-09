using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Soulful.Core.Services
{
    public class CardLoaderService : ICardLoaderService
    {
        public Dictionary<string, PackInfo> Packs { get; protected set; }

        public CardLoaderService()
        {
            // TODO - load pack info
        }

        public async Task<Tuple<string, int>> GetBlackCardAsync(int index)
        {
            throw new NotImplementedException();  
        }

        public Task<IEnumerable<Tuple<string, int>>> GetBlackCardsAsync(string packKey)
        {
            throw new NotImplementedException();
        }

        public async Task<string> GetWhiteCardAsync(int index)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<string>> GetWhiteCardsAsync(string packKey)
        {
            throw new NotImplementedException();
        }
    }

    public struct PackInfo
    {
        /// <summary>
        /// Gets the name of the pack
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// Gets the point at which the black cards for this pack start
        /// </summary>
        public int BlackStartRange { get; internal set; }

        /// <summary>
        /// Gets or sets the number of black cards in the pack
        /// </summary>
        public int BlackCount { get; internal set; }

        /// <summary>
        /// Gets the point at which the white cards for this pack start
        /// </summary>
        public int WhiteStartRange { get; internal set; }

        /// <summary>
        /// Gets the number of white cards in the pack
        /// </summary>
        public int WhiteCount { get; internal set; }
    }
}
