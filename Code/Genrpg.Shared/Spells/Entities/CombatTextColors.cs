using MessagePack;
namespace Genrpg.Shared.Spells.Entities
{
    [MessagePackObject]
    public class CombatTextColors
    {
        public const int White = 0;
        public const int Red = 1;
        public const int Blue = 2;
        public const int Orange = 3;
        public const int Yellow = 4;
        public const int Magenta = 5;
        public const int Black = 6;
        public const int Cyan = 7;
        public const int Green = 8;
    }
}
