using Genrpg.Shared.DataStores.Categories.PlayerData;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.DataStores.PlayerData
{
    public abstract class OwnerQuantityObjectList<TChild> : OwnerIdObjectList<TChild> where TChild : OwnerPlayerData, IOwnerQuantityChild, new()
    {
    }
}
