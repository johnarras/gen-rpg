using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Units.Entities
{
    public  class UnitRole
    {
        public long RoleId { get; set; }
        [Key(4)] public string Name { get; set; }
    }
}
