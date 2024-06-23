using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Units.Entities
{
    public  class UnitClass
    {
        public long ClassId { get; set; }
        public long Level { get; set; }
        [Key(4)] public string Name { get; set; }
    }
}
