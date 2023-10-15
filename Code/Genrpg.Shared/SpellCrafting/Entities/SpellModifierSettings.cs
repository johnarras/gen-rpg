using Genrpg.Shared.DataStores.GameSettings;
using Genrpg.Shared.GameSettings.Loading;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.SpellCrafting.Entities
{
    [MessagePackObject]
    public class SpellModifierSettings : ParentSettings<SpellModifier>
    {
        [Key(0)] public override string Id { get; set; }

        public SpellModifier GetSpellModifier(long idkey) { return _lookup.Get<SpellModifier>(idkey); }
    }

    [MessagePackObject]
    public class SpellModifierSettingsApi : ParentSettingsApi<SpellModifierSettings, SpellModifier> { }
    [MessagePackObject]
    public class SpellModifierSettingsLoader : ParentSettingsLoader<SpellModifierSettings, SpellModifier, SpellModifierSettingsApi> { }
}
