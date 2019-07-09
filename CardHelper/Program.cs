using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CardHelper
{
    public static class Program
    {
        public const string PACK_KEY_IDENTIFIER = "order";

        public const string BLACK_CARDS_IDENTIFIER = "blackCards";
        public const string WHITE_CARDS_IDENTIFIER = "whiteCards";

        public const string BLACK_CARDS_RANGE_IDENTIFIER = "black";
        public const string WHITE_CARDS_RANGE_IDENTIFIER = "white";
        public const string PACK_NAME_IDENTIFIER = "name";

        public static void Main(string[] args)
        {
            if (args.Length != 2)
                throw new ArgumentException("Required: pathToFile, pathToOutputDirectory");

            using (StreamReader reader = File.OpenText(args[0]))
            {
                JObject cardList = (JObject)JToken.ReadFrom(new JsonTextReader(reader));

                // Get all the packs in the file
                List<string> packKeys = (cardList[PACK_KEY_IDENTIFIER] as JArray)?.Select(k => k.ToString()).ToList();
                if (packKeys == null || packKeys.Count <= 0)
                    throw new Exception("The loaded file contains no pack keys");

                // Get all the cards
                List<JObject> blackCards = (cardList[BLACK_CARDS_IDENTIFIER] as JArray)?.Select(c => (JObject)c).ToList();
                if (blackCards == null || blackCards.Count <= 0)
                    throw new Exception("The loaded file contains no black cards");
                List<string> whiteCards = (cardList[WHITE_CARDS_IDENTIFIER] as JArray)?.Select(c => c.ToString()).ToList();
                if (whiteCards == null || whiteCards.Count <= 0)
                    throw new Exception("The loaded file contains no white cards");

                // Operate on each pack
                foreach (string key in packKeys)
                {
                    List<int> blackRange = (cardList[key][BLACK_CARDS_RANGE_IDENTIFIER] as JArray)?.Select(n => (int)n).ToList();
                    List<int> whiteRange = (cardList[key][WHITE_CARDS_RANGE_IDENTIFIER] as JArray)?.Select(n => (int)n).ToList();

                    string name = cardList[key][PACK_NAME_IDENTIFIER].ToString();

                    // Get black cards
                    List<JObject> blackCardsTake;
                    if (blackRange?.Count > 0)
                        blackCardsTake = blackCards.GetRange(blackRange[0], blackRange.Count).ToList();
                    else
                        blackCardsTake = new List<JObject>();

                    // Get white cards
                    List<string> whiteCardsTake;
                    if (whiteRange?.Count > 0)
                        whiteCardsTake = whiteCards.GetRange(whiteRange[0], whiteRange.Count).ToList();
                    else
                        whiteCardsTake = new List<string>();

                    SaveCardList(args[1], key, name, blackCardsTake, whiteCardsTake);
                }
            }
        }

        public static void SaveCardList(string path, string key, string name, List<JObject> blackCards, List<string> whiteCards)
        {
            string directory = Path.GetFullPath(path);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            string filePath = Path.Combine(directory, key + ".json");

            JObject json =
                new JObject(
                    new JProperty(PACK_NAME_IDENTIFIER, name),
                    new JProperty(BLACK_CARDS_IDENTIFIER,
                        new JArray(
                            from c in blackCards
                            select c
                            )),
                    new JProperty(WHITE_CARDS_IDENTIFIER,
                        new JArray(
                            from c in whiteCards
                            select new JValue(c))));

            File.WriteAllText(filePath, json.ToString());
            Console.WriteLine(name + " cards saved to " + filePath);
            Console.WriteLine("=========");
        }

        private static void PrintList<T>(List<T> list)
        {
            foreach (T element in list)
                Console.WriteLine("-->" + element);
        }
    }
}
