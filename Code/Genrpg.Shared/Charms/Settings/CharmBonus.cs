using Genrpg.Shared.DataStores.GameSettings;
using Genrpg.Shared.Interfaces;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Genrpg.Shared.Charms.Settings
{
    [MessagePackObject]
    public class CharmBonus : ChildSettings, IIdName
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public string NameId { get; set; }
        [Key(5)] public string Desc { get; set; }
        [Key(6)] public string Icon { get; set; }

        [Key(7)] public long EntityTypeId { get; set; }
        [Key(8)] public long EntityId { get; set; }

        [Key(9)] public long CharmUseId { get; set; }
        
        [Key(10)] public bool CheckBitValue { get; set; }
        [Key(11)] public long CheckBitCount { get; set; }
        [Key(12)] public long CheckBitsMatchTarget { get; set; }
       
        [Key(13)] public long CheckBitStartIndex { get; set; }
        [Key(14)] public long CheckBitSkip { get; set; }

        [Key(15)] public long BonusQuantityStart { get; set; }
        [Key(16)] public long BonusQuantitySkip { get; set; }

        [Key(17)] public long QuantityBitsCount { get; set; }
        [Key(18)] public long QuantityMod { get; set; }

        [Key(19)] public long QuantityBitSkip { get; set; }
        [Key(20)] public long QuantityStartBit { get; set; }
        [Key(21)] public long QuantityBonusType { get; set; }

    }
}
