using MessagePack;
using Genrpg.Shared.MapObjects.MapObjectAddons.Constants;
using Genrpg.Shared.MapObjects.MapObjectAddons.Entities;
using Genrpg.Shared.Quests.WorldData;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Quests.MapObjectAddons
{
    [MessagePackObject]
    public class QuestAddon : BaseMapObjectAddon
    {
        public override long GetAddonType() { return MapObjectAddonTypes.Vendor; }

        [Key(0)] public List<QuestType> Quests { get; set; } = new List<QuestType>();
    }
}
