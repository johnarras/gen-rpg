using MessagePack;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Spells.Entities
{
    [MessagePackObject]
    public class OldSpell : IId, IName
    {
        
        [Key(0)] public long IdKey { get; set; }
        [Key(1)] public string Name { get; set; }
        [Key(2)] public List<string> Prereqs { get; set; }
        [Key(3)] public long Pre1OldSpellId { get; set; }
        [Key(4)] public long Pre2OldSpellId { get; set; }
        public OldSpell()
        {
            Prereqs = new List<string>();
        }
    }
}
