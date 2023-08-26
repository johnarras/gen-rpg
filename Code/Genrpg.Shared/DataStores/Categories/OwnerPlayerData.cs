using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.DataStores.Categories
{
    public abstract class OwnerPlayerData : BasePlayerData, IStringOwnerId
    {
        [MessagePack.IgnoreMember]
        public abstract string OwnerId { get; set; }
    }
}
