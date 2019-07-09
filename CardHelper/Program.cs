using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CardHelper
{
    public static class Program
    {
        public const char SEPARATOR = '|';

        public const string PACK_KEY_IDENTIFIER = "order";

        public const string BLACK_CARDS_IDENTIFIER = "blackCards";
        public const string WHITE_CARDS_IDENTIFIER = "whiteCards";

        public const string BLACK_CARDS_RANGE_IDENTIFIER = "black";
        public const string WHITE_CARDS_RANGE_IDENTIFIER = "white";
        public const string PACK_NAME_IDENTIFIER = "name";

        private static string _saveDirectory;

        public static void Main(string[] args)
        {
            if (args.Length != 2)
                throw new ArgumentException("Required: pathToFile, pathToOutputDirectory");
            _saveDirectory = Path.GetFullPath(args[1]);
            if (!Directory.Exists(_saveDirectory))
                Directory.CreateDirectory(_saveDirectory);

            using (StreamReader reader = File.OpenText(args[0]))
            {
                JObject cardList = (JObject)JToken.ReadFrom(new JsonTextReader(reader));

                // Get all the packs in the file
                List<string> packKeys = (cardList[PACK_KEY_IDENTIFIER] as JArray)?.Select(k => k.ToString()).ToList();
                if (packKeys == null || packKeys.Count <= 0)
                    throw new Exception("The loaded file contains no pack keys");

                // Save all the black cards
                List<JObject> blackCards = (cardList[BLACK_CARDS_IDENTIFIER] as JArray)?.Select(c => (JObject)c).ToList();
                if (blackCards == null || blackCards.Count <= 0)
                    throw new Exception("The loaded file contains no black cards");
                else
                    SaveBlackCards(blackCards);

                List<string> whiteCards = (cardList[WHITE_CARDS_IDENTIFIER] as JArray)?.Select(c => c.ToString()).ToList();
                if (whiteCards == null || whiteCards.Count <= 0)
                    throw new Exception("The loaded file contains no white cards");
                else
                    SaveWhiteCards(whiteCards);

                // Save all the packs
                List<PackInfo> packInfos = new List<PackInfo>();
                foreach (string key in packKeys)
                {
                    List<int> blackRange = (cardList[key][BLACK_CARDS_RANGE_IDENTIFIER] as JArray)?.Select(n => (int)n).ToList();
                    List<int> whiteRange = (cardList[key][WHITE_CARDS_RANGE_IDENTIFIER] as JArray)?.Select(n => (int)n).ToList();
                    string name = cardList[key][PACK_NAME_IDENTIFIER].ToString();

                    PackInfo info = new PackInfo
                    {
                        Name = name,
                        Key = key,
                        BlackStartRange = blackRange.Count > 0 ? blackRange[0] : -1,
                        BlackCount = blackRange.Count > 0 ? blackRange.Count : -1,
                        WhiteStartRange = whiteRange.Count > 0 ? whiteRange[0] : -1,
                        WhiteCount = whiteRange.Count > 0 ? whiteRange.Count : -1
                    };
                    packInfos.Add(info);
                }
                SavePacks(packInfos);
            }
        }

        public static void SaveBlackCards(List<JObject> cards)
        {
            using (StreamWriter writer = new StreamWriter(Path.Combine(_saveDirectory, "black.txt"), false, Encoding.UTF8))
            {
                foreach (JObject card in cards)
                {
                    string entry = card["text"].ToString();
                    entry = PruneString(entry);
                    entry += SEPARATOR;
                    entry += card["pick"].ToString();
                    writer.WriteLine(entry);
                }
            }
        }

        public static void SaveWhiteCards(List<string> cards)
        {
            using (StreamWriter writer = new StreamWriter(Path.Combine(_saveDirectory, "white.txt"), false, Encoding.UTF8))
            {
                foreach (string card in cards)
                {
                    string pruned = PruneString(card);
                    writer.WriteLine(pruned);
                }
            }
        }

        public static void SavePacks(List<PackInfo> packs)
        {
            using (StreamWriter writer = new StreamWriter(Path.Combine(_saveDirectory, "packs.txt"), false, Encoding.UTF8))
            {
                foreach (PackInfo pack in packs)
                    writer.WriteLine(pack.ToString());
            }
        }

        public struct PackInfo
        {
            public string Key { get; set; }
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the point at which the black cards for this pack start
            /// </summary>
            public int BlackStartRange { get; set; }

            /// <summary>
            /// Gets or sets the number of black cards in the pack
            /// </summary>
            public int BlackCount { get; set; }

            /// <summary>
            /// Gets or sets the point at which the white cards for this pack start
            /// </summary>
            public int WhiteStartRange { get; set; }

            /// <summary>
            /// Gets or sets the number of white cards in the pack
            /// </summary>
            public int WhiteCount { get; set; }

            public override string ToString()
            {
                return Key + SEPARATOR
                    + Name + SEPARATOR
                    + BlackStartRange + SEPARATOR
                    + BlackCount + SEPARATOR
                    + WhiteStartRange + SEPARATOR
                    + WhiteCount;
            }
        }

        private static string PruneString(string s)
        {
            string mod = s.Replace("<br>", "/");
            mod = mod.Replace("&reg;", "®");
            mod = mod.Replace("&reg", "®");
            mod = mod.Replace("&copy;", "©");
            mod = mod.Replace("&copy", "©");
            mod = mod.Replace("&trade;", "™");
            mod = mod.Replace("&trade", "™");
            mod = mod.Replace("&Uuml;", "Ü");
            mod = mod.Trim('.');
            return mod;
        }
    }
}
