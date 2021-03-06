﻿using Soulful.Core.Model.Cards;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Soulful.Core.Services
{
    public interface ICardLoaderService
    {
        Task<List<Pack>> GetPacks();
        Task<BlackCard> GetBlackCardAsync(int id);
        Task<WhiteCard> GetWhiteCardAsync(int id);
    }
}
