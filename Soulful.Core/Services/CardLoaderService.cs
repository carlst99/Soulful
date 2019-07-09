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
        public const string BLACK_CARDS_FILENAME = "black.txt";
        public const string WHITE_CARDS_FILENAME = "white.txt";
        public const string PACK_INFO_FILENAME = "packs.txt";
        public const string RESOURCE_LOCATION = "Soulful.Core.Resources.Cards.";

        private readonly Assembly _resourceAssembly;

        public Dictionary<string, PackInfo> Packs { get; protected set; }

        public CardLoaderService()
        {
            _resourceAssembly = typeof(CardLoaderService).GetTypeInfo().Assembly;
            LoadPackInfo();
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

        private async void LoadPackInfo()
        {
            Packs = new Dictionary<string, PackInfo>();

            using (Stream packsResource = _resourceAssembly.GetManifestResourceStream(RESOURCE_LOCATION + PACK_INFO_FILENAME))
            using (StreamReader reader = new StreamReader(packsResource, Encoding.UTF8))
            {
                while (!reader.EndOfStream)
                {
                    PackInfo info = PackInfo.Parse(await reader.ReadLineAsync().ConfigureAwait(false), out string key);
                    Packs.Add(key, info);
                }
            }
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

        public static PackInfo Parse(string s, out string key)
        {
            string[] components = s.Split('|');
            if (components.Length != 6)
                throw new ArgumentException("Invalid string input");

            key = components[0];
            return new PackInfo
            {
                Name = components[1],
                BlackStartRange = int.Parse(components[2]),
                BlackCount = int.Parse(components[3]),
                WhiteStartRange = int.Parse(components[4]),
                WhiteCount = int.Parse(components[5]),
            };
        }
    }
}
