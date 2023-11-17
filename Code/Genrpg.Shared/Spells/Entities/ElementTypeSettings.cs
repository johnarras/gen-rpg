using Genrpg.Shared.DataStores.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Spells.Entities
{
    [MessagePackObject]
    public class ElementTypeSettings : ParentSettings<ElementType>
    {
        [Key(0)] public override string Id { get; set; }

        public ElementType GetElementType(long idkey) { return _lookup.Get<ElementType>(idkey); }
    }

    [MessagePackObject]
    public class ElementTypeSettingsApi : ParentSettingsApi<ElementTypeSettings, ElementType> { }
    [MessagePackObject]
    public class ElementTypeSettingsLoader : ParentSettingsLoader<ElementTypeSettings, ElementType, ElementTypeSettingsApi> { }



}
