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

        private IGameData _gameData = null;

        public string PickWord(IRandom rand, List<WeightedName> list, string excludeName = "", string excludePrefix = "", string excludeDesc = "")
        {
            if (list == null)
            {
                return "";
            }

            float totalWeight = 0;
            float chosenWeight = 0;

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
                                return PickDataListName(rand, list[l].Name);
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
                    chosenWeight = (float)(rand.NextDouble() * totalWeight);
                }


            }

            return "";

        }

        public string CombinePrefixSuffix(IRandom rand, string prefix, string suffix, float hyphenChance)
        {

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
                rand.NextDouble() < hyphenChance)
            {
                name = prefix + "-" + suffix;
            }
            return name;
        }

        public string PickNameListName(IRandom rand, string nameListName, string excludeName = "", string excludePrefix = "", string excludeDesc = "")
        {
            if (string.IsNullOrEmpty(nameListName))
            {
                return "";
            }


            NameList nl = _gameData.Get<NameSettings>(null).GetNameList(nameListName);

            if (nl != null && nl.Names != null && nl.Names.Count > 0)
            {
                return PickWord(rand, nl.Names, excludeName, excludePrefix, excludeDesc);
            }
            return nameListName;
        }


        // Do a secondary pick from a data list for this word.
        public string PickDataListName(IRandom rand, string name)
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

            string newname = PickItemName(rand, list, onlyShortNames);

            if (string.IsNullOrEmpty(newname))
            {
                return name;
            }

            return newname;

        }


        public string PickItemName(IRandom rand, List<IIndexedGameItem> items, bool onlyShortNames = false)
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


                    choice = rand.Next() % totalCount;
                }

            }

            return "";
        }

        public string GenOfTheName(IRandom rand, List<WeightedName> prefixes, List<WeightedName> suffixes, int avoidPrefixLength = 0)
        {
            if (prefixes == null || prefixes.Count < 1)
            {
                return "";
            }

            if (suffixes == null || suffixes.Count < 1)
            {
                return "";
            }

            string prefix = PickWord(rand, prefixes);
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

            string suffix = PickWord(rand, suffixes, excludeWord, excludePrefix);




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
