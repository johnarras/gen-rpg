using MessagePack;
using Genrpg.Shared.Spells.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using Genrpg.Shared.MapMessages;

namespace Genrpg.Shared.SpellCrafting.Messages
{
    [MessagePackObject]
    public sealed class OnCraftSpell : BaseMapApiMessage
    {
        [Key(0)] public Spell CraftedSpell { get; set; }
    }
}
