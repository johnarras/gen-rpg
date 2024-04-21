using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Names.Settings;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Zones.Settings;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using Genrpg.Shared.GameSettings;
namespace Genrpg.Shared.Names.Services
{
    public class NameGenService : INameGenService
    {

        private IGameData _gameData;
        public async Task Initialize(GameState gs, CancellationToken toke)
        {
            await Task.CompletedTask;
        }

        public string PickWord(GameState gs, List<WeightedName> list, long seed = 0, string excludeName = "", string excludePrefix = "", string excludeDesc = "")
        {
            if (list == null)
            {
                return "";
            }

            float totalWeight = 0;
            float chosenWeight = 0;

            if (seed == 0)
            {
                seed = gs.rand.Next();
            }

            for (int times = 0; times < 2; times++)
            {

                for (int l = 0; l < list.Count; l++)
                {
                    if (!list[l].Ignore && list[l].Weight > 0 &&
                        !string.IsNullOrEmpty(list[l].Name) &&
                        (string.IsNullOrEmpty(excludeName) ||
                        list[l].Name.IndexOf(excludeName) < 0) &&
                        (string.IsNullOrEmpty(excludePrefix) ||
                        list[l].Name.IndexOf(excludePrefix) != 0) &&
                        (string.IsNullOrEmpty(excludeDesc) ||
                        list[l].Desc.IndexOf(excludeDesc) < 0))
                    {
                        if (times == 0)
                        {
                            totalWeight += list[l].Weight;
                        }
                        else
                        {
                            chosenWeight -= list[l].Weight;
                            if (chosenWeight < 0)
                            {
                                return PickDataListName(gs, list[l].Name);
                            }
                        }
                    }
                }

                if (times == 0)
                {
                    if (totalWeight < 1)
                    {
                        return "";
                    }
                    MyRandom rand2 = new MyRandom(seed);
                    chosenWeight = (float)(rand2.NextDouble() * totalWeight);
                }


            }

            return "";

        }

        MyRandom doubleWordRand = new MyRandom();
        public string CombinePrefixSuffix(string prefix, string suffix, float hyphenChance, long seed)
        {

            MyRandom hyphenRand = null;
            if (seed > 0)
            {
                hyphenRand = new MyRandom(seed);
            }
            else
            {
                hyphenRand = doubleWordRand;
            }

            if (string.IsNullOrEmpty(prefix))
            {
                if (!string.IsNullOrEmpty(suffix))
                {
                    return suffix;
                }
                else
                {
                    return "";
                }
            }
            else if (string.IsNullOrEmpty(suffix))
            {
                return prefix;
            }

            // Both words exist.

            string lowerSuffix = suffix.ToLower();
            string name = prefix + lowerSuffix;

            if (prefix[prefix.Length - 1] == lowerSuffix[0] ||
                hyphenRand.NextDouble() < hyphenChance)
            {
                name = prefix + "-" + suffix;
            }
            return name;
        }

        public string PickNameListName(GameState gs, string nameListName, long seed = 0, string excludeName = "", string excludePrefix = "", string excludeDesc = "")
        {
            if (string.IsNullOrEmpty(nameListName))
            {
                return "";
            }


            NameList nl = _gameData.Get<NameSettings>(null).GetNameList(nameListName);

            if (nl != null && nl.Names != null && nl.Names.Count > 0)
            {
                return PickWord(gs, nl.Names, seed, excludeName, excludePrefix, excludeDesc);
            }
            return nameListName;
        }


        // Do a secondary pick from a data list for this word.
        public string PickDataListName(GameState gs, string name, long seed = 0)
        {
            if (string.IsNullOrEmpty(name))
            {
                return "";
            }


            List<IIndexedGameItem> list = new List<IIndexedGameItem>();



            bool onlyShortNames = false;
            if (name.IndexOf("Short") >= 0)
            {
                onlyShortNames = true;
            }
            else if (name.IndexOf("ZoneName") >= 0)
            {
                if (_gameData.Get<ZoneTypeSettings>(null).GetData() != null)
                {
                    foreach (ZoneType zt in _gameData.Get<ZoneTypeSettings>(null).GetData())
                    {
                        list.Add(zt);
                    }
                }
            }

            if (list == null || list.Count < 1)
            {
                return name;
            }

            string newname = PickItemName(gs, list, seed, onlyShortNames);

            if (string.IsNullOrEmpty(newname))
            {
                return name;
            }

            return newname;

        }


        public string PickItemName(GameState gs, List<IIndexedGameItem> items, long seed = 0, bool onlyShortNames = false)
        {
            int totalCount = 0;
            int choice = 0;
            for (int times = 0; times < 2; times++)
            {
                for (int i = 0; i < items.Count; i++)
                {
                    if (!string.IsNullOrEmpty(items[i].Name) &&
                        items[i].Name.ToLower() != "none" &&
                        (!onlyShortNames || StrUtils.NumVowelBlocks(items[i].Name) < 3))
                    {
                        if (times == 0)
                        {
                            totalCount++;
                        }
                        else
                        {
                            if (--choice < 0)
                            {
                                return items[i].Name;
                            }
                        }
                    }
                }

                if (times == 0)
                {
                    if (totalCount < 1)
                    {
                        return "";
                    }


                    MyRandom rand2 = null;
                    if (seed < 1)
                    {
                        rand2 = new MyRandom(gs.rand.Next());
                    }
                    else
                    {
                        rand2 = new MyRandom(seed);
                    }


                    choice = rand2.Next() % totalCount;
                }

            }

            return "";
        }

        public string GenOfTheName(GameState gs, List<WeightedName> prefixes, List<WeightedName> suffixes, long seed = 0, int avoidPrefixLength = 0)
        {
            MyRandom rand2 = null;
            if (seed == 0)
            {
                rand2 = new MyRandom(gs.rand.Next());
            }
            else
            {
                rand2 = new MyRandom(seed);
            }

            if (prefixes == null || prefixes.Count < 1)
            {
                return "";
            }

            if (suffixes == null || suffixes.Count < 1)
            {
                return "";
            }

            string prefix = PickWord(gs, prefixes, rand2.Next());
            string excludePrefix = "the ";
            // If the first ends in "of" allow "the suffix" suffixes
            if (!string.IsNullOrEmpty(prefix) && prefix.LastIndexOf(" of") >= 0 &&
                prefix.LastIndexOf(" of") == prefix.Length - 3)
            {
                excludePrefix = "";
            }

            string excludeWord = "";
            if (avoidPrefixLength > 0)
            {
                string wordToAvoid = prefix;
                if (wordToAvoid.Length > avoidPrefixLength)
                {
                    wordToAvoid = wordToAvoid.Substring(0, avoidPrefixLength);
                }
                excludeWord = wordToAvoid;
            }

            string txt3 = excludePrefix;

            string suffix = PickWord(gs, suffixes, rand2.Next(), excludeWord, excludePrefix);




            if (string.IsNullOrEmpty(prefix) || string.IsNullOrEmpty(suffix))
            {
                return "";
            }


            if (suffix.IndexOf("ing") == suffix.Length - 3 && prefix.IndexOf("ing") == prefix.Length - 3)
            {
                suffix = suffix.Substring(0, suffix.Length - 3);
            }

            return prefix + " " + suffix;

        }


    }
}
