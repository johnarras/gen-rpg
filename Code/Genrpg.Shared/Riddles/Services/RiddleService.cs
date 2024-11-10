using Genrpg.Shared.Client.Core;
using Genrpg.Shared.Crawler.Maps.Entities;
using Genrpg.Shared.Crawler.Maps.Settings;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.GameSettings.Interfaces;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Inventory.Settings.ItemTypes;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.Names.Settings;
using Genrpg.Shared.Riddles.Settings;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Utils.Data;
using Genrpg.Shared.Zones.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.Shared.Riddles.Services
{

    public interface IRiddleService : IInitializable
    {
        Task GenerateRiddles(List<CrawlerMap> floors, IRandom rand);
    }

    public class RiddleService : IRiddleService
    {
        private IGameData _gameData = null;
        private IClientGameState _gs = null;

        const int MinWordLength = 3;
        const int MaxLetterPosition = 6;
        const int MaxWordLength = 9;

        private bool _didInit = false;

        private Dictionary<int, Dictionary<char, List<string>>> _letterPositionWords = new Dictionary<int, Dictionary<char, List<string>>>();

        private Dictionary<int, List<string>> _wordsByLength = new Dictionary<int, List<string>>();

        private List<string> _allWords = new List<string>();

        private List<string> _itemNames = new List<string>();

        public async Task Initialize(CancellationToken token)
        {
            await Task.CompletedTask;
        }

        private void InitWords()
        {
            if (_didInit)
            {
                return;
            }
            _didInit = true;

            _letterPositionWords = new Dictionary<int, Dictionary<char, List<string>>>();
            _wordsByLength = new Dictionary<int, List<string>>();

            IReadOnlyList<NameList> nameLists = _gameData.Get<NameSettings>(null).GetData();

            foreach (NameList nl in nameLists)
            {
                foreach (WeightedName word in nl.Names)
                {
                   
                    string lowerword = word.Name.ToLower().Trim();

                    if (lowerword.Length >= MinWordLength && lowerword.Length <= MaxWordLength && !_allWords.Contains(lowerword))
                    {
                        _allWords.Add(lowerword);
                    }
                }
            }

            IReadOnlyList<ZoneType> zoneTypes = _gameData.Get<ZoneTypeSettings>(_gs.ch).GetData();

            List<String> allZoneTypeWords = new List<string>();
            foreach (ZoneType ztype in zoneTypes)
            {
                allZoneTypeWords.AddRange(ztype.CreatureNamePrefixes.Select(x => x.Name));
                allZoneTypeWords.AddRange(ztype.CreatureDoubleNamePrefixes.Select(x => x.Name));
                allZoneTypeWords.AddRange(ztype.ZoneAdjectives.Select(x => x.Name));
                allZoneTypeWords.AddRange(ztype.ZoneNames.Select(x => x.Name));
                allZoneTypeWords.AddRange(ztype.TreeTypes.Select(x => x.Name));
            }

            foreach (string ztypeWord in allZoneTypeWords)
            {
                if (ztypeWord == null)
                {
                    continue;
                }

                string normalizedName = ztypeWord.ToLower().Trim();

                if (normalizedName.Length >= MinWordLength && normalizedName.Length <= MaxWordLength && !_allWords.Contains(normalizedName))
                {
                    _allWords.Add(normalizedName);
                }

            }

            IReadOnlyList<ItemType> itemTypes = _gameData.Get<ItemTypeSettings>(_gs.ch).GetData();


            foreach (ItemType itype in itemTypes)
            {
                if (itype.EquipSlotId < 1)
                {
                    continue;
                }

                _itemNames.Add(itype.Name);

                if (itype.Names == null)
                {
                    continue;
                }

                foreach (WeightedName word in itype.Names)
                {
                    string lowerword = word.Name.ToLower().Trim();

                    if (lowerword.Length >= MinWordLength && lowerword.Length <= MaxWordLength && !_allWords.Contains(lowerword))
                    {
                        _allWords.Add(lowerword);
                        if (!_itemNames.Contains(word.Name))
                        {
                            _itemNames.Add(word.Name);
                        }
                    }
                }
            }



            //////////////////////////////////////////////
            /// END OF IMPORT -- NOW CREATE DICTIONARIES
            /// /////////////////////////////////////////

            for (int i = 0; i <= MaxLetterPosition; i++)
            {
                _letterPositionWords[i] = new Dictionary<char, List<string>>();
            }


            foreach (string word in _allWords)
            {
                if (word.Any(x => !char.IsLetterOrDigit(x)))
                {
                    continue;
                }

                if (!_wordsByLength.ContainsKey(word.Length))
                {
                    _wordsByLength[word.Length] = new List<string>();
                }
                _wordsByLength[word.Length].Add(word);

                for (int i = 0; i < MaxLetterPosition - 1; i++)
                {
                    Dictionary<char, List<string>> posDict = _letterPositionWords[i];

                    if (i < word.Length)
                    {

                        if (!posDict.ContainsKey(word[i]))
                        {
                            posDict[word[i]] = new List<string>();
                        }

                        posDict[word[i]].Add(word);
                    }
                }
            }
        }


        public async Task GenerateRiddles(List<CrawlerMap> floors, IRandom rand)
        {

            InitWords();
            long minFloor = Math.Max(2, floors.Min(x => x.MapFloor));
            long maxFloor = floors.Max(x => x.MapFloor);

            CrawlerMapType mapType = _gameData.Get<CrawlerMapSettings>(_gs.ch).Get(floors[0].CrawlerMapTypeId);

            for (long floorChosen = minFloor; floorChosen < maxFloor; floorChosen++)
            {
                if (rand.NextDouble() > mapType.RiddleUnlockChance)
                {
                    continue;
                }

                CrawlerMap lockedFloor = floors.FirstOrDefault(x => x.MapFloor == floorChosen);

                if (lockedFloor == null)
                {
                    continue;
                }

                CrawlerMap prevFloor = floors.FirstOrDefault(x => x.MapFloor == floorChosen - 1);

                if (prevFloor == null)
                {
                    continue;
                }

                List<PointXZ> openPoints = new List<PointXZ>();

                for (int x = 0; x < prevFloor.Width; x++)
                {
                    for (int z = 0; z < prevFloor.Height; z++)
                    {
                        if (prevFloor.Get(x, z, CellIndex.Terrain) < 1 ||
                           prevFloor.Get(x, z, CellIndex.Encounter) > 0 ||
                            prevFloor.Get(x, z, CellIndex.Magic) > 0 ||
                            prevFloor.Get(x, z, CellIndex.Disables) > 0)
                        {
                            continue;
                        }

                        MapCellDetail detail = prevFloor.Details.FirstOrDefault(d => d.X == x && d.Z == z);

                        if (detail != null)
                        {
                            continue;
                        }

                        openPoints.Add(new PointXZ(x, z));
                    }
                }

                if (openPoints.Count < 20)
                {
                    continue;
                }


                int choice = rand.Next(1, 5);

                if (choice == 1)
                {
                    AddBasicRiddle(lockedFloor, prevFloor, openPoints, rand);
                }
                else if (choice == 2)
                {
                    AddPositionWordFind(lockedFloor, prevFloor, openPoints, rand);
                }
                else if (choice == 3)
                {
                    AddWordLengthPuzzle(lockedFloor, prevFloor, openPoints, rand);
                }
                else if (choice == 4)
                {
                    AddFirstLetterPuzzle(lockedFloor, prevFloor, openPoints, rand);
                }
                else if (choice == 5)
                {
                    AddMathPuzzle(lockedFloor, prevFloor, openPoints, rand);
                }

            }

            await Task.CompletedTask;
        }

        private void AddBasicRiddle(CrawlerMap lockedFloor, CrawlerMap prevFloor, List<PointXZ> openPoints, IRandom rand)
        {
            IReadOnlyList<Riddle> riddles = _gameData.Get<RiddleSettings>(_gs.ch).GetData();

            if (riddles.Count < 1)
            {
                return;
            }

            StringBuilder riddleText = new StringBuilder();
            riddleText.Append("Answer the following to pass:\n\n");

            Riddle riddle = riddles[rand.Next(riddles.Count)];  

            string[] lines = riddle.Desc.Split('\n');

            for (int l = 0; l < lines.Length; l++)
            {
                if (!String.IsNullOrEmpty(lines[l]))
                {

                    riddleText.Append(lines[l].Substring(0, 3) + ".......\n");
                    StringBuilder clueText = new StringBuilder();

                    clueText.Append("Some strange writing is on the wall...\n\n");

                    clueText.Append(lines[l] + "\n\n");
                    for (int i = 0; i < riddle.Name.Length; i++)
                    {
                        if (i == (lockedFloor.IdKey*97 + l * 31) % riddle.Name.Length)
                        {
                            clueText.Append(riddle.Name[i]);
                        }
                        else
                        {
                            clueText.Append("?");
                        }
                    }
                    clueText.Append("\n\n");

                    PointXZ openPoint = openPoints[rand.Next(openPoints.Count)];
                    prevFloor.Details.Add(new MapCellDetail()
                    {
                        EntityTypeId = EntityTypes.Riddle,
                        EntityId = riddle.IdKey,
                        X = (int)openPoint.X,
                        Z = (int)openPoint.Z,
                        Index = l,
                        Text = clueText.ToString(),
                    });
                }
            }

            riddleText.Append("\n\nWhat am I?\n\n");

            riddleText.Append("\n");
            for (int i = 0; i < riddle.Name.Length; i++)
            {
                riddleText.Append("?");
            }
           
            lockedFloor.RiddleText = riddleText.ToString();
            lockedFloor.RiddleAnswer = riddle.Name.ToLower().Trim();
            lockedFloor.RiddleError = "Sorry, that is not correct. Look around for clues...";
        }

        private List<string> _offsetWords = new List<string>()
        {
            "first",
            "second",
            "third",
            "fourth",
            "fifth",
            "sixth",
            "seventh",
        };


        private void AddPositionWordFind(CrawlerMap lockedFloor, CrawlerMap prevFloor, List<PointXZ> openPoints, IRandom rand)
        {

            string wordChosen = _allWords[rand.Next(_allWords.Count)];


            List<MapCellDetail> clueDetails = new List<MapCellDetail>();

            List<string> clueWords = new List<string>();


            for (int l = 0; l < wordChosen.Length; l++)
            {

                List<string> wordChoices = new List<string>();
                int startOffsetIndex = MathUtils.IntRange(0, MaxLetterPosition, rand);

                if (rand.NextDouble() < 0.7f)
                {
                    startOffsetIndex /= 2;
                }

                string okWord = null;

                string offsetName = _offsetWords[startOffsetIndex];


                for (int idx = startOffsetIndex; idx >= 0; idx--)
                {

                    if (!_letterPositionWords.ContainsKey(idx))
                    {
                        continue;
                    }

                    Dictionary<char, List<string>> offsetDict = _letterPositionWords[idx];

                    if (offsetDict.TryGetValue(wordChosen[l], out List<string> words))
                    {
                        if (words.Count < 1)
                        {
                            continue;
                        }

                        for (int times = 0; times < 4; times++)
                        {
                            okWord = words[rand.Next(words.Count)];

                            if (okWord == wordChosen || clueWords.Contains(okWord))
                            {
                                okWord = null;
                                continue;
                            }
                            else
                            {
                                break;
                            }
                        }

                        if (okWord != null)
                        {
                            offsetName = _offsetWords[idx];
                            break;
                        }
                    }
                }

                if (okWord == null)
                {
                    return;
                }

                clueWords.Add(okWord);


                StringBuilder clueText = new StringBuilder();

                clueText.Append("A word is written in blood:\n\n");

                clueText.Append(okWord + "\n\n");

                clueText.Append($"The {offsetName} letter is heavily scratched....\n\n");

                PointXZ openPoint = openPoints[rand.Next(openPoints.Count)];
                clueDetails.Add(new MapCellDetail()
                {
                    EntityTypeId = EntityTypes.Riddle,
                    X = (int)openPoint.X,
                    Z = (int)openPoint.Z,
                    Index = l,
                    Text = clueText.ToString(),
                });

            }

            if (clueDetails.Count != wordChosen.Length)
            {
                return;
            }

            prevFloor.Details.AddRange(clueDetails);

            StringBuilder riddleText = new StringBuilder();

            riddleText.Append("You must speak the proper word to pass.\n\n");

            riddleText.Append("Search these corridors for the scrawls\n\n");

            riddleText.Append("of madmen who failed to answer the riddle.\n\n");

            riddleText.Append($"Hint: The answer has {wordChosen.Length} letters.\n\n");

            lockedFloor.RiddleText = riddleText.ToString();
            lockedFloor.RiddleAnswer = wordChosen;
            lockedFloor.RiddleError = "Sorry, that is not the proper word to pass...";
        }

        private void AddFirstLetterPuzzle(CrawlerMap lockedFloor, CrawlerMap prevFloor, List<PointXZ> openPoints, IRandom rand)
        {
            Dictionary<char, List<string>> dict = _letterPositionWords[0];

            List<char> keys = dict.Keys.ToList();

            int wordCount = 7;

            List<char> chosenLetters = new List<char>();

            for (int i = 0; i < wordCount; i++)
            {
                char c = keys[rand.Next() % keys.Count];

                keys.Remove(c);
                chosenLetters.Add(c);
            }

            List<string> riddleWords = new List<string>();

            chosenLetters = chosenLetters.OrderBy(x => x).ToList();

            int middlePos = wordCount / 2;
            char middle = chosenLetters[middlePos];
            char prev = chosenLetters[middlePos - 1];
            char next = chosenLetters[middlePos + 1];

            foreach (char c in chosenLetters)
            {
                List<string> words = dict[c];

                riddleWords.Add(words[rand.Next() % words.Count]);
            }

            string answer = riddleWords[middlePos];
            riddleWords[middlePos] = "????????";

            List<char> badAnswerChars = keys.Where(x => x < prev || x > next).ToList();

            int wrongAnswerCount = 3;

            List<string> otherAnswers = new List<string>();

            for (int i = 0; i < wrongAnswerCount; i++)
            {
                char badLetter = badAnswerChars[rand.Next() % badAnswerChars.Count];

                otherAnswers.Add(dict[badLetter][rand.Next() % dict[badLetter].Count]);
            }

            otherAnswers.Insert(rand.Next() % otherAnswers.Count, answer);

            StringBuilder riddleText = new StringBuilder();

            riddleText.Append("Select the word that fits properly within the following sequence:\n\n");

            for (int i = 0; i < riddleWords.Count; i++)
            {
                riddleText.Append(riddleWords[i] + " ");
            }

            riddleText.Append("\n\n");

            riddleText.Append("Your choices are:\n\n");

            for (int i = 0; i < otherAnswers.Count; i++)
            {
                riddleText.Append(otherAnswers[i] + " ");   
            }

            riddleText.Append("\n\n");

            riddleText.Append("Which one fits the best in the sequence?");

            lockedFloor.RiddleText = riddleText.ToString();
            lockedFloor.RiddleAnswer = answer;
            lockedFloor.RiddleError = "Sorry, that is not the correct answer...";


        }


        private void AddWordLengthPuzzle(CrawlerMap lockedFloor, CrawlerMap prevFloor, List<PointXZ> openPoints, IRandom rand)
        {
            Dictionary<int, List<string>> dict = _wordsByLength;

            List<int> keys = dict.Keys.OrderBy(x => x).ToList();

            int wordCount = 5;

            List<int> chosenLengths = new List<int>();

            for (int i = 0; i < wordCount; i++)
            {
                int len = keys[rand.Next() % keys.Count];

                keys.Remove(len);
                chosenLengths.Add(len);
            }

            List<string> riddleWords = new List<string>();

            chosenLengths = chosenLengths.OrderBy(x => x).ToList();

            int middlePos = wordCount / 2;
            int middle = chosenLengths[middlePos];
            int prev = chosenLengths[middlePos - 1];
            int next = chosenLengths[middlePos + 1];

            foreach (int len in chosenLengths)
            {
                List<string> words = dict[len];

                riddleWords.Add(words[rand.Next() % words.Count]);
            }

            string answer = riddleWords[middlePos];

            riddleWords[middlePos] = "????????";

            List<int> badAnswerChars = keys.Where(x => x < prev || x > next).ToList();

            int wrongAnswerCount = 3;

            List<string> otherAnswers = new List<string>();

            for (int i = 0; i < wrongAnswerCount; i++)
            {
                if (badAnswerChars.Count < 1)
                {
                    return;
                }

                int badLength = badAnswerChars[rand.Next() % badAnswerChars.Count];

                otherAnswers.Add(dict[badLength][rand.Next() % dict[badLength].Count]);
            }

            otherAnswers.Insert(rand.Next() % otherAnswers.Count, answer);

            StringBuilder riddleText = new StringBuilder();

            riddleText.Append("Select the word that fits best within the following sequence:\n\n");

            for (int i = 0; i < riddleWords.Count; i++)
            {
                riddleText.Append(riddleWords[i] + " ");
            }

            riddleText.Append("\n\n");

            riddleText.Append("Your choices are:\n\n");

            for (int i = 0; i < otherAnswers.Count; i++)
            {
                riddleText.Append(otherAnswers[i] + " ");
            }

            riddleText.Append("\n\n");

            riddleText.Append("Which one fits the best in the sequence?");

            lockedFloor.RiddleText = riddleText.ToString();
            lockedFloor.RiddleAnswer = answer;
            lockedFloor.RiddleError = "Sorry, that is not the correct answer...";



        }


        private void AddMathPuzzle(CrawlerMap lockedFloor, CrawlerMap prevFloor, List<PointXZ> openPoints, IRandom rand)
        {
            if (_itemNames.Count < 1)
            {
                return;
            }

            int itemQuantity = rand.Next(2, 3);

            List<string> itemNameDupe = new List<string>(_itemNames);

            List<string> wordNames = new List<string>();

            for (int i = 0; i < itemQuantity; i++)
            {
                string newWord = itemNameDupe[rand.Next() % itemNameDupe.Count];
                wordNames.Add(newWord);
               itemNameDupe.Remove(newWord);
            }

            StringBuilder riddleSb = new StringBuilder();

            riddleSb.Append("Alice has the following work order:\n\n");

            long total = 0;

            for (int i = 0; i < wordNames.Count; i++)
            {
                long purchaseQuantity = MathUtils.LongRange(3, 10, rand);
                long purchaseCost = MathUtils.LongRange(3, 10, rand);

                total += purchaseQuantity * purchaseCost;

                riddleSb.Append(purchaseQuantity + " " + wordNames[i] + " costing " + purchaseCost + " gold each.\n\n");
            }

            long totalGold = (100 * ((total / 100) + 1));

            riddleSb.Append("And they have " + totalGold + " gold currently.\n\n");

            riddleSb.Append("How much gold will they have after completing this purchase?\n\n");

            lockedFloor.RiddleText = riddleSb.ToString();
            lockedFloor.RiddleAnswer = (totalGold - total).ToString();
            lockedFloor.RiddleError = "Sorry, that's not the correct amount!";
               

        }
    }
}
