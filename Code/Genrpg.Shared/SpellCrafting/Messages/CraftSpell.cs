using MessagePack;
using Genrpg.Shared.Spells.Entities;
using Genrpg.Shared.MapMessages;

namespace Genrpg.Shared.SpellCrafting.Messages
{
    [MessagePackObject]
    public sealed class CraftSpell : BaseMapApiMessage, IPlayerCommand
    {
        [Key(0)] public Spell CraftedSpell { get; set; }
    }
}
