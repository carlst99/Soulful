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
        private List<Tuple<string, int>> _blackCards;
        private List<string> _whiteCards;

        public List<PackInfo> Packs { get; protected set; }

        public CardLoaderService()
        {
            _resourceAssembly = typeof(CardLoaderService).GetTypeInfo().Assembly;
            LoadPackInfo();
        }

        public async Task<Tuple<string, int>> GetBlackCardAsync(int index)
        {
            if (_blackCards == null)
                await LoadWhiteCards().ConfigureAwait(false);

            return _blackCards[index];
        }

        public async Task<List<Tuple<string, int>>> GetPackBlackCardsAsync(string packKey)
        {
            if (_blackCards == null)
                await LoadBlackCards().ConfigureAwait(false);

            PackInfo info = Packs.Find(p => p.Key == packKey);
            if (info.Equals(default))
                throw new ArgumentException("A pack with that key does not exist");

            return _blackCards.GetRange(info.BlackStartRange, info.BlackCount);
        }

        public async Task<string> GetWhiteCardAsync(int index)
        {
            if (_whiteCards == null)
                await LoadWhiteCards().ConfigureAwait(false);

            return _whiteCards[index];
        }

        public async Task<List<string>> GetPackWhiteCardsAsync(string packKey)
        {

            if (_whiteCards == null)
                await LoadWhiteCards().ConfigureAwait(false);

            PackInfo info = Packs.Find(p => p.Key == packKey);
            if (info.Equals(default))
                throw new ArgumentException("A pack with that key does not exist");

            return _whiteCards.GetRange(info.WhiteStartRange, info.WhiteCount);
        }

        private async void LoadPackInfo()
        {
            Packs = new List<PackInfo>();

            using (Stream packsResource = _resourceAssembly.GetManifestResourceStream(RESOURCE_LOCATION + PACK_INFO_FILENAME))
            using (StreamReader reader = new StreamReader(packsResource, Encoding.UTF8))
            {
                while (!reader.EndOfStream)
                {
                    PackInfo info = PackInfo.Parse(await reader.ReadLineAsync().ConfigureAwait(false));
                    Packs.Add(info);
                }
            }
        }

        private async Task LoadBlackCards()
        {
            _blackCards = new List<Tuple<string, int>>();

            using (Stream packsResource = _resourceAssembly.GetManifestResourceStream(RESOURCE_LOCATION + BLACK_CARDS_FILENAME))
            using (StreamReader reader = new StreamReader(packsResource, Encoding.UTF8))
            {
                while (!reader.EndOfStream)
                {
                    string card = await reader.ReadLineAsync().ConfigureAwait(false);
                    string[] components = card.Split('|');
                    components[0] = components[0].Replace("/", "\n");
                    _blackCards.Add(new Tuple<string, int>(components[0], int.Parse(components[1])));
                }
            }
        }

        private async Task LoadWhiteCards()
        {
            _whiteCards = new List<string>();

            using (Stream packsResource = _resourceAssembly.GetManifestResourceStream(RESOURCE_LOCATION + WHITE_CARDS_FILENAME))
            using (StreamReader reader = new StreamReader(packsResource, Encoding.UTF8))
            {
                while (!reader.EndOfStream)
                    _whiteCards.Add(await reader.ReadLineAsync().ConfigureAwait(false));
            }
        }
    }

    public struct PackInfo
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
