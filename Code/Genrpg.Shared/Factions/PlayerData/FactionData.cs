using MessagePack;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Units.Entities;
using System.Collections.Generic;
using System.Linq;
using Genrpg.Shared.Spells.Casting;
using Genrpg.Shared.Utils;
using Genrpg.Shared.DataStores.PlayerData;
using Genrpg.Shared.Factions.Constants;
using Genrpg.Shared.Factions.Settings;
using Genrpg.Shared.Factions.Messages;

namespace Genrpg.Shared.Factions.PlayerData
{
    /// <summary>
    /// A list of affinities/reputations
    /// </summary>
    /// 
    [MessagePackObject]
    public class FactionData : OwnerIdObjectList<FactionStatus>
    {
        [Key(0)] public override string Id { get; set; }

        /// <summary>
        ///  Find the FactionStub for a given faction, 
        /// </summary>
        /// <param name="factionTypeId">The faction to find</param>
        /// <param name="createIfNull">Create if null or return null if missing</param>
        /// <returns>The Reputation found or null</returns>
        public FactionStatus Find(GameState gs, long factionTypeId)
        {

            FactionStatus currStatus = _data.FirstOrDefault(x => x.IdKey == factionTypeId);
            if (currStatus == null)
            {
                currStatus = new FactionStatus()
                {
                    IdKey = factionTypeId,
                    RepLevelId = RepLevels.Neutral,
                    Id = HashUtils.NewGuid(),
                    OwnerId = Id,
                };
                _data.Add(currStatus);
                currStatus.SetDirty(true);
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
        public RepResult SetRep(GameState gs, Unit unit, int factionId, long val)
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
                RepLevel repLevel = gs.data.GetGameData<ReputationSettings>(unit).GetRepLevel(status.RepLevelId - 1);
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
                RepLevel repLevel = gs.data.GetGameData<ReputationSettings>(unit).GetRepLevel(status.RepLevelId);


                if (repLevel == null || status.Reputation <= repLevel.PointsNeeded)
                {
                    break;
                }

                int pointsNeeded = repLevel.PointsNeeded;

                repLevel = gs.data.GetGameData<ReputationSettings>(unit).GetRepLevel(status.RepLevelId + 1);
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

            if (res.OldRep != res.NewRep || res.OldRepLevelId != res.NewRepLevelId)
            {
                status.SetDirty(true);
            }

            return res;
        }



        /// <summary>
        /// Add reputation to a faction
        /// </summary>
        /// <param name="factionId">The faction to add to</param>
        /// <param name="val">The amount to add</param>
        public RepResult Add(GameState gs, Unit unit, int factionId, long val)
        {
            return SetRep(gs, unit, factionId, GetRep(gs, factionId) + val);
        }
    }
}
