using Genrpg.Shared.DataStores.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Spells.Entities
{
    [MessagePackObject]
    public class SkillTypeSettings : ParentSettings<SkillType>
    {
        [Key(0)] public override string Id { get; set; }

        public SkillType GetSkillType(long idkey) { return _lookup.Get<SkillType>(idkey); }
    }

    [MessagePackObject]
    public class SkillTypeSettingsApi : ParentSettingsApi<SkillTypeSettings, SkillType> { }
    [MessagePackObject]
    public class SkillTypeSettingsLoader : ParentSettingsLoader<SkillTypeSettings, SkillType, SkillTypeSettingsApi> { }


}
