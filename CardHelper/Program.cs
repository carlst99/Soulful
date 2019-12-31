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

                IQueryable<WhiteCard> realmWhiteCards = _cardsRealm.All<WhiteCard>();
                IQueryable<BlackCard> realmBlackCards = _cardsRealm.All<BlackCard>();
                _cardsRealm.Write(() =>
                {
                    int id = 0;
                    foreach (string key in packKeys)
                    {
                        List<int> blackRange = (cardList[key][BLACK_CARDS_RANGE_IDENTIFIER] as JArray)?.Select(n => (int)n).ToList();
                        List<int> whiteRange = (cardList[key][WHITE_CARDS_RANGE_IDENTIFIER] as JArray)?.Select(n => (int)n).ToList();
                        string name = cardList[key][PACK_NAME_IDENTIFIER].ToString();

                        Pack pack = new Pack
                        {
                            Id = id,
                            Name = name
                        };

                        foreach (int element in blackRange)
                        {
                            string cardContent = PruneString(blackCards[element]["text"].ToString());
                            pack.BlackCards.Add(realmBlackCards.First(c => c.Content == cardContent));
                        }

                        foreach (int element in whiteRange)
                        {
                            string cardContent = PruneString(whiteCards[element]);
                            pack.WhiteCards.Add(realmWhiteCards.First(c => c.Content == cardContent));
                        }

                        _cardsRealm.Add(pack);
                        id++;
                    }
                });
                Console.WriteLine("Packs saved to DB");
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
