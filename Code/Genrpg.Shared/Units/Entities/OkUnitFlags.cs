using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Units.Entities
{
    [MessagePackObject]
    public class OkUnitFlags
    {
        public const long PlayersOk = 1 << 0;
        public const long DeletedOk = 2 << 0;

    }
}
