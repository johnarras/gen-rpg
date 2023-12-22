using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.DataStores.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;

namespace Genrpg.Shared.Spells.Settings.Spells
{
    [MessagePackObject]
    public class SpellTypeSettings : ParentSettings<SpellType>
    {
        [Key(0)] public override string Id { get; set; }

        public SpellType GetSpellType(long idkey) { return _lookup.Get<SpellType>(idkey); }
    }

    [MessagePackObject]
    public class SpellTypeSettingsApi : ParentSettingsApi<SpellTypeSettings, SpellType> { }
    [MessagePackObject]
    public class SpellTypeSettingsLoader : ParentSettingsLoader<SpellTypeSettings, SpellType, SpellTypeSettingsApi> { }
}
