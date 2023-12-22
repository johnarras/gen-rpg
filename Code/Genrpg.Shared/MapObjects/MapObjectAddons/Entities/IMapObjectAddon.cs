using System;
using System.Collections.Generic;
using System.Text;
using Genrpg.Shared.DataStores.Interfaces;
using Genrpg.Shared.MapObjects.MapObjectAddons.Constants;
using Genrpg.Shared.Movement.Messages;
using Genrpg.Shared.Quests.MapObjectAddons;
using Genrpg.Shared.Vendors.MapObjectAddons;
using MessagePack;

namespace Genrpg.Shared.MapObjects.MapObjectAddons.Entities
{
    // Used for addons to a map object
    // Note: For serialization purposes all implementations must do the Union thing here.

    [Union((int)MapObjectAddonTypes.Vendor, typeof(VendorAddon))]
    [Union((int)MapObjectAddonTypes.Quest, typeof(QuestAddon))]
    public interface IMapObjectAddon
    {
        long GetAddonType();
    }
}
