
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Names.Entities;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Zones.Entities;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Spells.Entities;
using System.Threading;
using Genrpg.Shared.Spells.Interfaces;
using Genrpg.Shared.MapObjects.Entities;

namespace Genrpg.Shared.Interfaces
{

    /// <summary>
    /// This is used to mark everything that is considered a service
    /// This will be used to turn setup into a 2 step process where all
    /// services are put into a service loc and then for each service,
    /// all other serviecs needed will be looked up at startup and
    /// then used from then on instead of using the loc
    /// </summary>
    public interface IService
    {
    }

    // Used for services that need to have a "setup" function run at startup.
    public interface ISetupService : IService
    {
        Task Setup(GameState gs, CancellationToken token);
    }


    /// <summary>
    /// Use this to set up dictionaries of classes for things like handlers such that the 
    /// interface must contain some key.
    /// </summary>
    public interface ISetupDictionaryItem<T>
    {
        T GetKey();
    }

    public interface IFactorySetupService
    {
        void Setup(GameState gs);
    }


    public interface IUnitGenService : IService
    {
        string GenerateUnitPrefixName(GameState gs, long unitTypeId, Zone zone, IRandom rand,
            Dictionary<string, string>? args = null);

        UnitType GetRandomUnitType(GameState gs, Map map, Zone zone);

        string GenerateUnitName(GameState gs, long unitTypeId, long zoneId, IRandom rand,
            Dictionary<string, string>? args = null);

    }

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
