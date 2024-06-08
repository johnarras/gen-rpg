using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Names.Settings;
using Genrpg.Shared.Utils;
using System.Collections.Generic;

namespace Genrpg.Shared.Names.Services
{

    public interface INameGenService : IInitializable
    {
        string PickWord(IRandom rand, List<WeightedName> list, string excludeName = "", string excludePrefix = "", string excludeDesc = "");
        string PickDataListName(IRandom rand, string name);
        string PickNameListName(IRandom rand, string nameListName, string excludeName = "", string excludePrefix = "", string excludeDesc = "");
        string PickItemName(IRandom rand, List<IIndexedGameItem> list, bool onlyShortNames = false);

        string CombinePrefixSuffix(IRandom rand, string prefix, string suffix, float hyphenChance);

        // Gen names of the following form.
        // prefix suffix.
        // If prefix is of the form "prefix of",
        // then allow suffixes of the form "the suffix",
        // otherwise don't allow suffixes of the form "the suffix"
        string GenOfTheName(IRandom rand, List<WeightedName> prefixes, List<WeightedName> suffixes, int avoidMatchingPrefixLength = 0);
    }

}
