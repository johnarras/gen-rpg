using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loading;
using Genrpg.Shared.DataStores.GameSettings;
using Genrpg.Shared.Interfaces;

namespace Genrpg.Shared.Entities.Settings
{
    [MessagePackObject]
    public class EntitySettings : ParentSettings<EntityType>
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override List<EntityType> Data { get; set; }

        public EntityType GetEntityType(long idkey) { return _lookup.Get<EntityType>(idkey); }
    }

    [MessagePackObject]
    public class EntityType : ChildSettings, IIndexedGameItem
    {
        public const long None = 0;
        public const long Currency = 1;
        public const long Item = 2;
        public const long Spell = 3;
        public const long Unit = 4;
        public const long Spawn = 5;
        public const long Scaling = 7;
        public const long Recipe = 8;
        public const long Quest = 9;
        public const long Set = 10;
        public const long NPC = 11;
        public const long QuestItem = 12;
        public const long GroundObject = 13;
        public const long Zone = 14;
        public const long ZoneUnit = 15;
        public const long ProxyCharacter = 16;

        public const long Stat = 31;
        public const long StatPct = 32;
        public const long Damage = 33;
        public const long Healing = 34;
        public const long Shield = 35;


        // User Reward Types
        public const long UserCoin = 200;

        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public string Desc { get; set; }
        [Key(5)] public string Icon { get; set; }
        [Key(6)] public string Art { get; set; }
    }

    [MessagePackObject]
    public class EntitySettingsApi : ParentSettingsApi<EntitySettings, EntityType> { }

    [MessagePackObject]
    public class EntitySettingsLoader : ParentSettingsLoader<EntitySettings, EntityType, EntitySettingsApi> { }

}
