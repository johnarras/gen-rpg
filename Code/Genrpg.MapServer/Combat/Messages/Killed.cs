using Genrpg.Shared.MapMessages;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.MapServer.Combat.Messages
{
    public sealed class Killed : BaseMapMessage
    {
        public string UnitId { get; set; }
        public long UnitTypeId { get; set; }
        public long NPCTypeId { get; set; }
        public long FactionTypeId { get; set; }
        public long Level { get; set; }
        public long ZoneId { get; set; }
    }
}