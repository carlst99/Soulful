using Soulful.Core.Model.Cards;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Soulful.Core.Services
{
    public class CardLoaderService : ICardLoaderService
    {
        protected const string BLACK_CARDS_FILENAME = "black.txt";
        protected const string WHITE_CARDS_FILENAME = "white.txt";
        protected const string PACK_INFO_FILENAME = "packs.txt";
        protected const string RESOURCE_LOCATION = "Soulful.Core.Resources.Cards.";

        private readonly Assembly _resourceAssembly;
        private Dictionary<int, BlackCard> _blackCards;
        private Dictionary<int, WhiteCard> _whiteCards;

        public List<Pack> Packs { get; protected set; }

        public CardLoaderService()
        {
            _resourceAssembly = typeof(CardLoaderService).GetTypeInfo().Assembly;
            LoadPacks();
        }

        public async Task<BlackCard> GetBlackCardAsync(int id)
        {
            if (_blackCards == null)
                await LoadBlackCards().ConfigureAwait(false);

            return _blackCards[id];
        }

        public async Task<WhiteCard> GetWhiteCardAsync(int index)
        {
            if (_whiteCards == null)
                await LoadWhiteCards().ConfigureAwait(false);

            return _whiteCards[index];
        }

        private async Task LoadPacks()
        {
            Packs = new List<Pack>();
            await LoadBlackCards().ConfigureAwait(false);
            await LoadWhiteCards().ConfigureAwait(false);

            using Stream packsResource = _resourceAssembly.GetManifestResourceStream(RESOURCE_LOCATION + PACK_INFO_FILENAME);
            using StreamReader reader = new StreamReader(packsResource, Encoding.UTF8);
            for (int packId = 0; !reader.EndOfStream; packId++)
            {
                PackInfo info = PackInfo.Parse(await reader.ReadLineAsync().ConfigureAwait(false));
                Pack pack = new Pack
                {
                    Id = packId,
                    Name = info.Name
                };
                for (int i = info.BlackStartRange; i < info.BlackStartRange + info.BlackCount; i++)
                    pack.BlackCards.Add(_blackCards[i]);
                for (int i = info.WhiteStartRange; i < info.WhiteStartRange + info.WhiteCount; i++)
                    pack.WhiteCards.Add(_whiteCards[i]);
                Packs.Add(pack);
            }
        }

        private async Task LoadBlackCards()
        {
            _blackCards = new Dictionary<int, BlackCard>();

            using Stream packsResource = _resourceAssembly.GetManifestResourceStream(RESOURCE_LOCATION + BLACK_CARDS_FILENAME);
            using StreamReader reader = new StreamReader(packsResource, Encoding.UTF8);
            for (int cardId = 0; !reader.EndOfStream; cardId++)
            {
                string card = await reader.ReadLineAsync().ConfigureAwait(false);
                string[] components = card.Split('|');
                components[0] = components[0].Replace("/", "\n");
                _blackCards.Add(cardId, new BlackCard(cardId, components[0], int.Parse(components[1])));
            }
        }

        private async Task LoadWhiteCards()
        {
            _whiteCards = new Dictionary<int, WhiteCard>();

            using Stream packsResource = _resourceAssembly.GetManifestResourceStream(RESOURCE_LOCATION + WHITE_CARDS_FILENAME);
            using StreamReader reader = new StreamReader(packsResource, Encoding.UTF8);
            for (int cardId = 0; !reader.EndOfStream; cardId++)
                _whiteCards.Add(cardId, new WhiteCard(cardId, await reader.ReadLineAsync().ConfigureAwait(false)));
        }
    }

    /// <summary>
    /// Provides a direct interpretation of the card resource structure
    /// </summary>
    internal struct PackInfo
    {
        /// <summary>
        /// Gets the key of the pack
        /// </summary>
        public string Key { get; internal set; }

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

        public static PackInfo Parse(string s)
        {
            string[] components = s.Split('|');
            if (components.Length != 6)
                throw new ArgumentException("Invalid string input");

            return new PackInfo
            {
                Key = components[0],
                Name = components[1],
                BlackStartRange = int.Parse(components[2]),
                BlackCount = int.Parse(components[3]),
                WhiteStartRange = int.Parse(components[4]),
                WhiteCount = int.Parse(components[5])
            };
        }

        public override string ToString()
        {
            return Name;
        }

        public override bool Equals(object obj)
        {
            return obj is PackInfo info
                && info.Key == Key;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                const int hash = 17;
                return (hash * 23) + Key.GetHashCode();
            }
        }
    }
}
