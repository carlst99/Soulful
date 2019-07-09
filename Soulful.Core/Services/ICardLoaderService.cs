using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Soulful.Core.Services
{
    public interface ICardLoaderService
    {
        Dictionary<string, PackInfo> Packs { get; }

        Task<IEnumerable<Tuple<string, int>>> GetBlackCardsAsync(string packKey);
        Task<IEnumerable<string>> GetWhiteCardsAsync(string packKey);

        Task<Tuple<string, int>> GetBlackCardAsync(int index);
        Task<string> GetWhiteCardAsync(int index);
    }
}
