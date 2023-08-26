using MessagePack;
namespace Genrpg.Shared.Constants
{
    /// <summary>
    /// Rpg Constants
    /// </summary>
    [MessagePackObject]
    public class RpgConstants
    {


        /// <summary>
        /// Time spent in combat after an action is taken.
        /// </summary>
        public const float InCombatTime = 4.5f;


        public const string DefaultMapItemArt = "TreasureChest";



        public const float WeaponDamageMinMult = 0.75f;
        public const float WeaponDamageMaxMult = 1.25f;


        public const string DefaultItemIcon = "Ring_001";


        public const int BaseStat = 10;


        public const int PetRegenPenaltyPct = 25;


    }

}
