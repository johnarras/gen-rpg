using MessagePack;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.DataStores.Core;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Units.Entities;
using System.Collections.Generic;
using System.Linq;
using Genrpg.Shared.Spells.Entities;

namespace Genrpg.Shared.Factions.Entities
{
    /// <summary>
    /// A list of affinities/reputations
    /// </summary>
    /// 
    [MessagePackObject]
    public class FactionData : ObjectList<FactionStatus>
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override List<FactionStatus> Data { get; set; } = new List<FactionStatus>();
        public override void AddTo(Unit unit) { unit.Set(this); }
        public override void Delete(IRepositorySystem repoSystem) { repoSystem.Delete(this); }
        /// <summary>
        ///  returns the list of affinities
        /// </summary>
        /// <returns>The list of affinities</returns>
        public List<FactionStatus> FindAll()
        {
            return Data;
        }

        /// <summary>
        ///  Find the FactionStub for a given faction, 
        /// </summary>
        /// <param name="factionTypeId">The faction to find</param>
        /// <param name="createIfNull">Create if null or return null if missing</param>
        /// <returns>The Reputation found or null</returns>
        public FactionStatus Find(GameState gs, long factionTypeId)
        {

            FactionStatus currStatus = Data.FirstOrDefault(x => x.IdKey == factionTypeId);
            if (currStatus == null)
            {
                currStatus = new FactionStatus() { IdKey = factionTypeId, RepLevelId = RepLevel.Neutral };
                FactionType factionType = gs.data.GetGameData<FactionSettings>().GetFactionType(factionTypeId);
                if (factionType != null && factionType.StartRepLevelId > RepLevel.None)
                {
                    currStatus.RepLevelId = factionType.StartRepLevelId;
                }
                Data.Add(currStatus);
            }
            return currStatus;
        }

        /// <summary>
        /// Return the rep level we have with this faction.
        /// </summary>
        /// <param name="factionId"></param>
        /// <returns></returns>
        public long GetLevel(GameState gs, long factionId)
        {
            return Find(gs, factionId).RepLevelId;
        }

        /// <summary>
        /// Get an FactionStub value based on faction Id, or 0 if missing
        /// </summary>
        /// <param name="factionId">The faction Id to find</param>
        /// <returns>The FactionStub value or 0 if missing</returns>
        public long GetRep(GameState gs, long factionId)
        {
            return Find(gs, factionId).Reputation;
        }

        /// <summary>
        /// Set a reputtation value
        /// </summary>
        /// <param name="factionId">Which faction Id to set</param>
        /// <param name="val">The value to set</param>
        public RepResult SetRep(GameState gs, int factionId, long val)
        {
            FactionStatus status = Find(gs, factionId);
            RepResult res = new RepResult()
            {
                OldRep = status.Reputation,
                OldRepLevelId = status.RepLevelId,
            };
            long diff = val - status.Reputation;

            res.RepChange = diff;

            status.Reputation = val;

            // Negative rep, go down levels.
            while (status.Reputation < 0)
            {
                RepLevel repLevel = gs.data.GetGameData<FactionSettings>().GetRepLevel(status.RepLevelId - 1);
                if (repLevel == null || status.RepLevelId <= 1)
                {
                    status.Reputation = 0;
                    break;
                }
                else
                {
                    status.RepLevelId = repLevel.IdKey;
                    status.Reputation = repLevel.PointsNeeded - val;
                }
            }

            while (true)
            {
                RepLevel repLevel = gs.data.GetGameData<FactionSettings>().GetRepLevel(status.RepLevelId);


                if (repLevel == null || status.Reputation <= repLevel.PointsNeeded)
                {
                    break;
                }

                int pointsNeeded = repLevel.PointsNeeded;

                repLevel = gs.data.GetGameData<FactionSettings>().GetRepLevel(status.RepLevelId + 1);
                if (repLevel == null)
                {
                    status.Reputation = pointsNeeded - 1;
                    break;
                }
                else
                {
                    status.RepLevelId = repLevel.IdKey;
                    status.Reputation -= pointsNeeded;
                }
            }
            res.NewRepLevelId = status.RepLevelId;
            res.NewRep = status.Reputation;
            return res;
        }



        /// <summary>
        /// Add reputation to a faction
        /// </summary>
        /// <param name="factionId">The faction to add to</param>
        /// <param name="val">The amount to add</param>
        public RepResult Add(GameState gs, int factionId, long val)
        {
            return SetRep(gs, factionId, GetRep(gs, factionId) + val);
        }
    }
}
