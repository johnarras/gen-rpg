using MessagePack;
namespace Genrpg.Shared.Units.Constants
{
    [MessagePackObject]
    public class UnitConstants
    {
        public const float ErrorDistance = 1000000;

        public const int PlayerObjectVisRadius = 2;

        public const float CombatSpeedMult = 1.2f;

        public const float EvadeSpeedMult = 1.5f;

        public const float RotSpeedPerFrame = 20.0f;

        public const float GridChangeCooldownSeconds = 5.0f;

        public const float CorpseDespawnSeconds = 60.0f;


    }
}
