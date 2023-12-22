using MessagePack;
using Genrpg.Shared.MapMessages;
using Genrpg.Shared.Spells.PlayerData.Spells;

namespace Genrpg.Shared.SpellCrafting.Messages
{
    [MessagePackObject]
    public sealed class CraftSpell : BaseMapApiMessage, IPlayerCommand
    {
        [Key(0)] public Spell CraftedSpell { get; set; }
    }
}
