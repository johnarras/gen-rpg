using MessagePack;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.DataStores.GameSettings;

namespace Genrpg.Shared.Stats.Settings.Stats
{   /// <summary>
    /// Stats have current core stats:
    /// Health/Mana/Might/Intellect/Willpower/Agility
    /// </summary>
    [MessagePackObject]
    public class StatType : ChildSettings, IIndexedGameItem
    {


        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public string Abbrev { get; set; }
        [Key(5)] public string Desc { get; set; }
        [Key(6)] public string Icon { get; set; }
        [Key(7)] public string Art { get; set; }

        [Key(8)] public int MaxPool { get; set; }
        [Key(9)] public int RegenSeconds { get; set; }
        [Key(10)] public int GenScalePct { get; set; }

    }


    public static class StatExtensionMethods
    {
    }

}
