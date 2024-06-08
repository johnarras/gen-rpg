using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Factions.Constants;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Units.Entities;
using System.Threading.Tasks;
using System.Threading;
using Genrpg.Shared.Factions.PlayerData;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Factions.Messages;
using Genrpg.Shared.Factions.Settings;
using Genrpg.Shared.Utils;
using System.Xml.Linq;
using Genrpg.Shared.DataStores.Entities;
using System.Linq;

namespace Genrpg.Shared.Factions.Services
{

    public interface ISharedFactionService : IInitializable
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

        public async Task Initialize(IGameState gs, CancellationToken toke)
        {
            await Task.CompletedTask;
        }

        private FactionStatus Find(Unit unit, long factionTypeId)
        {
            return unit.Get<FactionData>().Get(factionTypeId);
        }

        public long GetRep(Unit unit, long factionTypeId)
        {
            return Find(unit, factionTypeId).Reputation;
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

            FactionStatus status = Find(unit, factionTypeId); 
            long diff = quantity - status.Reputation;
            

            if (diff == 0)
            {
                return;        
            }

            status.Reputation = quantity;

            RepLevel repLevel = _gameData.Get<ReputationSettings>(unit).GetData().FirstOrDefault(x => x.PointsNeeded <= status.Reputation);

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
