using MessagePack;
namespace Genrpg.Shared.Spells.Entities
{
    [MessagePackObject]
    public class SpellConstants
    {

        public const int CastTimeTickMS = 500;
        public const int ShotMS = 500;
        public const int LinkedTargetBeamMS = 300;
        public const float BaseTickSeconds = 10.0f;
        public const float DotTickSeconds = 2.0f;
        public const float ProjectileSpeed = 50.0f;
        public const int BaseScale = 100;
        public const int BaseProcDepth = 0;

        public const int ExtraTargetRadius = 8;
        public const int SpellRadiusMult = 5;

        public const float ResendProjectileDelaySec = 0.25f;
        public const float ResendInstantDelaySec = 0.1f;
        /// <summary>
        /// Only allow 2 cooldown spells for a given elem/skill pair to avoid cheese.
        /// </summary>
        public const int MaxCooldownSpellsPerElemSkillPair = 2;


        public const int MaxProcDepth = 3;


        public const int MaxProcsPerSpell = 3;

        public const int GlobalCooldownMS = 1000;


        public const int MaxCustomSpellTypeId = 999;
        public const int MinGlobalSpellTypeId = 1000;

        public const int BasicAttackSpellTypeId = 1001;


        public float DurationPctCapForFullRefresh = 0.25f;

        public const int MaxDefenseMult = 80;

        public const int MaxRegenReductionPct = 75;


        public const float ProjectileOffsetDist = 2.0f;
        public const float FXOffsetDist = 2.0f;

    }
}
