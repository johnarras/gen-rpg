using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Names.Settings;
using System.Collections.Generic;

namespace Genrpg.Shared.Names.Services
{

    public interface INameGenService : IService
    {
        string PickWord(GameState gs, List<WeightedName> list, long seed, string excludeName = "", string excludePrefix = "", string excludeDesc = "");
        string PickDataListName(GameState gs, string name, long seed = 0);
        string PickNameListName(GameState gs, string nameListName, long seed = 0, string excludeName = "", string excludePrefix = "", string excludeDesc = "");
        string PickItemName(GameState gs, List<IIndexedGameItem> list, long seed = 0, bool onlyShortNames = false);

        string CombinePrefixSuffix(string prefix, string suffix, float hyphenChance, long seed);

        // Gen names of the following form.
        // prefix suffix.
        // If prefix is of the form "prefix of",
        // then allow suffixes of the form "the suffix",
        // otherwise don't allow suffixes of the form "the suffix"
        string GenOfTheName(GameState gs, List<WeightedName> prefixes, List<WeightedName> suffixes, long seed = 0, int avoidMatchingPrefixLength = 0);
    }

}
