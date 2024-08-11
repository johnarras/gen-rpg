using Genrpg.Shared.Factions.Constants;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Factions.Settings;
using System.Linq;
using Genrpg.Shared.Factions.PlayerData;

namespace Genrpg.Shared.Factions.Services
{

    public interface ISharedFactionService : IInjectable
    {
        long GetRepLevel(Unit unit, long factionTypeId);
        bool CanInteract(Unit unit, long factionTypeId);
        bool CanFight(Unit unit, long factionTypeId);
        bool WillAttack(Unit unit, long factionTypeId);
        long GetRep(Unit unit, long factionTypeId);
        void SetRep(Unit unit, long factionTypeId, long val);
        void AddRep(Unit unit, long factionTypeId, long val);
    }

    public class SharedFactionService : ISharedFactionService
    {
        private IGameData _gameData = null;

        private ReputationStatus Find(Unit unit, long factionTypeId)
        {
            return unit.Get<ReputationData>().Get(factionTypeId);
        }

        public long GetRep(Unit unit, long factionTypeId)
        {
            return Find(unit, factionTypeId).Quantity;
        }

        public long GetRepLevel(Unit unit, long factionTypeId)
        {
            return Find(unit, factionTypeId).RepLevelId;
        }

        public bool CanInteract(Unit unit, long factionTypeId)
        {
            return GetRepLevel(unit, factionTypeId) >= RepLevels.Neutral;
        }

        public bool CanFight(Unit unit, long factionTypeId)
        {
            return GetRepLevel(unit, factionTypeId) <= RepLevels.Unfriendly;
        }

        public bool WillAttack(Unit unit, long factionTypeId)
        {
            return GetRepLevel(unit, factionTypeId) <= RepLevels.Hostile;
        }

        public void AddRep(Unit unit, long factionTypeId, long quantity)
        {
            SetRep(unit, factionTypeId, quantity + GetRep(unit, factionTypeId));
        }

        public void SetRep(Unit unit, long factionTypeId, long quantity)
        {
            if (quantity < 0)
            {
                quantity = 0;
            }

            ReputationStatus status = Find(unit, factionTypeId); 
            long diff = quantity - status.Quantity;
            

            if (diff == 0)
            {
                return;        
            }

            status.Quantity = quantity;

            RepLevel repLevel = _gameData.Get<ReputationSettings>(unit).GetData().FirstOrDefault(x => x.PointsNeeded <= status.Quantity);

            if (repLevel == null)
            {
                status.RepLevelId = 1;
            }
            else
            {
                status.RepLevelId = repLevel.IdKey;
            }
        }
    }
}
