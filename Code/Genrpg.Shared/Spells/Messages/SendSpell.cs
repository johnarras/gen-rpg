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
    public sealed class SendSpell : BaseMapApiMessage
    {

        [Key(0)] public string CasterId { get; set; }
        [Key(1)] public long CasterLevel { get; set; }
        [Key(2)] public long CasterFactionId { get; set; }
        [Key(3)] public ElementType ElementType { get; set; }
        [Key(4)] public SkillType SkillType { get; set; }
        [Key(5)] public long ElementRank { get; set; }
        [Key(6)] public long SkillRank { get; set; }
        [Key(7)] public StatGroup CasterStats { get; set; }
        [Key(8)] public Spell Spell { get; set; }

        public SendSpell()
        {
            ElementRank = AbilityData.DefaultRank;
            SkillRank = AbilityData.DefaultRank;
        }
    }
}
