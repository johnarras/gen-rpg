using Genrpg.Shared.Charms.PlayerData;
using Genrpg.Shared.Units.Loaders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Charms.Loaders
{
    public class CrafterDataLoader : OwnerDataLoader<PlayerCharmData, PlayerCharm, PlayerCharmApi>
    {
        protected override bool IsUserData() { return true; }
    }
}
