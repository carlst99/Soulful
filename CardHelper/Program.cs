using CardHelper.CardDb;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Realms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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

        private static Realm _cardsRealm;

        public static void Main(string[] args)
        {
            if (args.Length != 2)
                throw new ArgumentException("Required: pathToFile, pathToOutputDirectory");

            string saveDirectory = Path.GetFullPath(args[1]);
            if (!Directory.Exists(saveDirectory))
                Directory.CreateDirectory(saveDirectory);

            RealmConfiguration config = new RealmConfiguration(Path.Combine(saveDirectory, "cards.realm"));
            Realm.DeleteRealm(config);
            _cardsRealm = Realm.GetInstance(config);

            using (StreamReader reader = File.OpenText(args[0]))
            {
                JObject cardList = (JObject)JToken.ReadFrom(new JsonTextReader(reader));

                // Get all the packs in the file
                List<string> packKeys = (cardList[PACK_KEY_IDENTIFIER] as JArray)?.Select(k => k.ToString()).ToList();
                if (packKeys == null || packKeys.Count == 0)
                    throw new Exception("The loaded file contains no pack keys");

                // Save all the black cards
                List<JObject> blackCards = (cardList[BLACK_CARDS_IDENTIFIER] as JArray)?.Select(c => (JObject)c).ToList();
                if (blackCards == null || blackCards.Count == 0)
                    throw new Exception("The loaded file contains no black cards");
                else
                    SaveBlackCards(blackCards);

                List<string> whiteCards = (cardList[WHITE_CARDS_IDENTIFIER] as JArray)?.Select(c => c.ToString()).ToList();
                if (whiteCards == null || whiteCards.Count == 0)
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
                NormalisePackIndexes(packInfos);
                _cardsRealm.Refresh();
                SavePacks(packInfos);
            }
        }

        public static void SaveBlackCards(List<JObject> cards)
        {
            _cardsRealm.Write(() =>
            {
                int id = 0;
                foreach (JObject card in cards)
                {
                    BlackCard blackCard = new BlackCard
                    {
                        Id = id,
                        Content = PruneString(card["text"].ToString()),
                        NumPicks = int.Parse(card["pick"].ToString())
                    };
                    _cardsRealm.Add(blackCard);
                    id++;
                }
            });
            Console.WriteLine("Black cards saved to DB");
        }

        public static void SaveWhiteCards(List<string> cards)
        {
            _cardsRealm.Write(() =>
            {
                int id = 0;
                foreach (string card in cards)
                {
                    WhiteCard whiteCard = new WhiteCard
                    {
                        Id = id,
                        Content = PruneString(card)
                    };
                    _cardsRealm.Add(whiteCard);
                    id++;
                }
            });
            Console.WriteLine("White cards saved to DB");
        }

        public static void NormalisePackIndexes(List<PackInfo> packInfos)
        {
            int lastBlackIndex = 0;
            int lastWhiteIndex = 0;

            for (int i = 0; i < packInfos.Count; i++)
            {
                PackInfo info = packInfos[i];
                if (info.BlackStartRange != -1 && info.BlackCount != -1)
                {
                    info.BlackStartRange = lastBlackIndex;
                    lastBlackIndex += info.BlackCount;
                }
                if (info.WhiteStartRange != -1 && info.WhiteCount != -1)
                {
                    info.WhiteStartRange = lastWhiteIndex;
                    lastWhiteIndex += info.WhiteCount;
                }
            }
        }

        public static void SavePacks(List<PackInfo> packs)
        {
            Queue<WhiteCard> whiteCards = new Queue<WhiteCard>(_cardsRealm.All<WhiteCard>());
            Queue<BlackCard> blackCards = new Queue<BlackCard>(_cardsRealm.All<BlackCard>());

            _cardsRealm.Write(() =>
            {
                int id = 0;
                foreach (PackInfo info in packs)
                {
                    Pack pack = new Pack
                    {
                        Id = id,
                        Name = info.Name
                    };
                    if (info.BlackStartRange != -1 && info.BlackCount != -1)
                    {
                        for (int i = info.BlackStartRange; i < info.BlackStartRange + info.BlackCount; i++)
                            pack.BlackCards.Add(blackCards.Dequeue());
                    }
                    if (info.WhiteStartRange != -1 && info.WhiteCount != -1)
                    {
                        for (int i = info.WhiteStartRange; i < info.WhiteStartRange + info.WhiteCount; i++)
                            pack.WhiteCards.Add(whiteCards.Dequeue());
                    }
                    _cardsRealm.Add(pack);
                    id++;
                }
            });
            Console.WriteLine("Packs saved to DB");
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
            mod = mod.Replace("_", "________");
            mod = mod.Trim('.');
            return mod;
        }
    }
}
