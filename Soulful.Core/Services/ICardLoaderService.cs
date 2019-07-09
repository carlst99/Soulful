using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Soulful.Core.Services
{
    public interface ICardLoaderService
    {
        Dictionary<string, PackInfo> Packs { get; }

        Task<List<Tuple<string, int>>> GetPackBlackCardsAsync(string packKey);
        Task<List<string>> GetPackWhiteCardsAsync(string packKey);

        Task<Tuple<string, int>> GetBlackCardAsync(int index);
        Task<string> GetWhiteCardAsync(int index);
    }
}
