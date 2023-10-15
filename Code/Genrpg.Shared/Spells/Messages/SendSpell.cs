using MessagePack;
using Genrpg.Shared.Spells.Entities;
using Genrpg.Shared.Stats.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using Genrpg.Shared.MapMessages;

namespace Genrpg.Shared.Spells.Messages
{
    [MessagePackObject]
    public sealed class SendSpell : BaseMapMessage
    {

        [Key(0)] public string CasterId { get; set; }
        [Key(1)] public string CasterGroupId { get; set; }
        [Key(2)] public long CasterLevel { get; set; }
        [Key(3)] public long CasterFactionId { get; set; }
        [Key(4)] public StatGroup CasterStats { get; set; }
        [Key(5)] public Spell Spell { get; set; }
        [Key(6)] public ElementType ElementType { get; set; }

        public SendSpell()
        {
        }
    }
}
