using Genrpg.Shared.Characters.Entities;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Factions.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Units.Entities;

namespace Genrpg.Shared.Factions.Services
{

    public interface ISharedFactionService : IService
    {
        long GetRepLevel(GameState gs, Unit unit, long factionTypeId);
        bool CanInteract(GameState gs, Unit unit, long factionTypeId);
        bool CanFight(GameState gs, Unit unit, long factionTypeId);
        bool WillAttack(GameState gs, Unit unit, long factionTypeId);
    }

    public class SharedFactionService : ISharedFactionService
    {
        /// <summary>
        /// Returns the relative rep level for the two units.
        /// 
        /// If both are characters, it's either hated or exalted based on same faction id (need more later)
        /// If both are not characters, it's based on whether the factions are the same or different.
        /// 
        /// If there's exactly one character, return the player's rep level vs that monster's faction.
        /// 
        /// </summary>
        /// <param name="gs"></param>
        /// <param name="unit1"></param>
        /// <param name="unit2"></param>
        /// <returns></returns>
        public long GetRepLevel(GameState gs, Unit unit, long factionTypeId)
        {

            if (unit == null || unit.FactionTypeId < 0 || factionTypeId < 0)
            {
                return RepLevel.Hated;
            }

            Character ch = unit as Character;

            if (ch == null)
            {
                if (unit.FactionTypeId == factionTypeId)
                {
                    return RepLevel.Exalted;
                }

                return RepLevel.Hated;
            }

            if (unit.FactionTypeId == factionTypeId)
            {
                return RepLevel.Exalted;
            }

            else
            {
                return RepLevel.Hated;
            }
            // return ch.Factions.GetLevel(gs, factionTypeId);
        }

        public bool CanInteract(GameState gs, Unit unit, long factionTypeId)
        {
            return GetRepLevel(gs, unit, factionTypeId) >= RepLevel.Neutral;
        }

        public bool CanFight(GameState gs, Unit unit, long factionTypeId)
        {
            return GetRepLevel(gs, unit, factionTypeId) <= RepLevel.Unfriendly;
        }

        public bool WillAttack(GameState gs, Unit unit, long factionTypeId)
        {
            return GetRepLevel(gs, unit, factionTypeId) <= RepLevel.Hostile;
        }

    }
}
