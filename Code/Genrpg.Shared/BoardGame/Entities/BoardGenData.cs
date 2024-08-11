using MessagePack;
using Genrpg.Shared.BoardGame.Constants;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.BoardGame.Entities
{
    [MessagePackObject]
    public class BoardGenData
    {
        [Key(0)] public long BoardModeId { get; set; } = BoardModes.Primary;
        [Key(1)] public long ForceZoneTypeId { get; set; } = 0;
        [Key(2)] public long EntityId { get; set; } // In some cases force certain bonus maps
        [Key(3)] public string OwnerId { get; set; } // Possible other owner id.
        [Key(4)] public long Seed { get; set; }
    }
}
