using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.DataStores.Categories.PlayerData
{
    public abstract class OwnerQuantityChild : OwnerPlayerData, IOwnerQuantityChild
    {

        [MessagePack.IgnoreMember]
        public abstract long IdKey { get; set; }

        [MessagePack.IgnoreMember]
        public abstract long Quantity { get; set; }
    }
}
