using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.PlayerData;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.DataStores.Categories.PlayerData
{
    public abstract class OwnerPlayerData : BasePlayerData, IStringOwnerId, IChildUnitData
    {
        [MessagePack.IgnoreMember]
        public abstract string OwnerId { get; set; }
    }
}
