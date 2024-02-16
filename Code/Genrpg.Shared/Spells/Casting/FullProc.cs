using Genrpg.Shared.Spells.Messages;
using MessagePack;

namespace Genrpg.Shared.Spells.Casting
{
    [MessagePackObject]
    public class FullProc
    {
        [Key(0)] public SpellHit SpellHit { get; set; }
        [Key(1)] public OldSpellProc Proc { get; set; }
        [Key(2)] public CurrentProc Current { get; set; }
    }
}
