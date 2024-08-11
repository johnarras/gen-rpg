using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Spells.Constants;
using Genrpg.Shared.Spells.PlayerData;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Genrpg.Shared.Spells.Services
{
    public class CombatAbilityService : ICombatAbilityService
    {
        private IRepositoryService _repoService = null;

        public void AddRank(Unit unit, long abilityCategoryId, long abilityTypeId, int points)
        {
            SetRank(unit, abilityCategoryId, points, GetRank(unit, abilityCategoryId, abilityTypeId) + points);
        }

        public int GetRank(Unit unit, long abilityCategoryId, long abilityTypeId)
        {
            CombatAbilityRank ab = GetRankItem(unit, abilityCategoryId, abilityTypeId);
            return ab.Rank;
        }

        public void SetRank(Unit unit, long abilityCategoryId, long abilityTypeId, int rank)
        {
            CombatAbilityRank abilityRank = GetRankItem(unit, abilityCategoryId, abilityTypeId);
            long oldRank = abilityRank.Rank;
            abilityRank.Rank = Math.Max(1, rank);
            if (abilityRank.Rank != oldRank)
            {
                _repoService.QueueSave(abilityRank);
            }
        }

        protected CombatAbilityRank GetRankItem(Unit unit, long abilityCategoryId, long abilityTypeId)
        {

            CombatAbilityData abilityData = unit.Get<CombatAbilityData>();

            CombatAbilityRank abilityRank = abilityData.GetData().FirstOrDefault(x => x.AbilityCategoryId == abilityCategoryId && x.AbilityTypeId == abilityTypeId);
            if (abilityRank == null)
            {
                lock (abilityData)
                {
                    abilityRank = abilityData.GetData().FirstOrDefault(x => x.AbilityCategoryId == abilityCategoryId && x.AbilityTypeId == abilityTypeId);
                    if (abilityRank == null)
                    {
                        abilityRank = new CombatAbilityRank()
                        {
                            Id = HashUtils.NewGuid(),
                            OwnerId = unit.Id,
                            AbilityCategoryId = abilityCategoryId,
                            AbilityTypeId = abilityTypeId,
                            Rank = AbilityConstants.DefaultRank,
                        };
                        List<CombatAbilityRank> ranks = abilityData.GetData().ToList();
                        ranks.Add(abilityRank);
                        abilityData.SetData(ranks);
                        _repoService.QueueSave(abilityRank);
                    }
                }
            }
            return abilityRank;
        }
    }
}
