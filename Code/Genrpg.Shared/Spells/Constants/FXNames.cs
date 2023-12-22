using MessagePack;
namespace Genrpg.Shared.Spells.Constants
{
    [MessagePackObject]
    public class FXNames
    {
        public const string Cast = "Cast";
        public const string DoT = "DoT";
        public const string Projectile = "Projectile";
        public const string SpellHit = "Blast";
        public const string MeleeHit = "Hit";
        public const string Shield = "Shield";
        public const string Explosion = "Explosion";
        public const string AllyHit = "Enchant";
        public const string Channel = "Channel";
    }
}
